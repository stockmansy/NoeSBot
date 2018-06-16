using Discord;
using Discord.Audio;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Models;
using NoeSbot.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class AudioPlayer
    {
        private IVoiceChannel _voiceChannel;
        private ITextChannel _textChannel;
        private IAudioClient _currentAudioChannel;
        private CancellationTokenSource _disposeToken;
        private ConcurrentQueue<AudioInfo> _queue;
        private bool _skip;
        private ulong _guildId;
        private float _volume;
        private int _count;
        private AudioStatusEnum _status;
        private bool _adding;
        private TaskCompletionSource<bool> _tcs;
        private bool _internalPause;
        
        public AudioPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel, ulong guildId, int defaultVolume = 5)
        {
            _voiceChannel = voiceChannel ?? throw new ArgumentNullException(nameof(voiceChannel));
            _textChannel = textChannel ?? throw new ArgumentNullException(nameof(textChannel));
            _disposeToken = new CancellationTokenSource();
            _queue = new ConcurrentQueue<AudioInfo>();
            _skip = false;
            _guildId = guildId;
            _volume = 1.0f;
            _status = AudioStatusEnum.Created;
            _tcs = new TaskCompletionSource<bool>();

            SetVolume(defaultVolume);
        }

        private bool Pause
        {
            get => _internalPause;
            set
            {
                new Thread(() => _tcs.TrySetResult(value)).Start();
                _internalPause = value;
            }
        }

        private string FileName => $"botsong_{_guildId}_{_count}";

        public ulong CurrentVoiceChannel => _voiceChannel.Id;

        public AudioStatusEnum Status { get => _status; }

        public void SetVolume(int volumeLevel)
        {
            if (volumeLevel < 0) volumeLevel = 0;
            if (volumeLevel > 10) volumeLevel = 10;

            _volume = (volumeLevel * 10) / 100.0f;
        }

        public async Task<AudioInfo> Start(List<string> items)
        {
            _currentAudioChannel = await _voiceChannel.ConnectAsync();

            var url = items.First();
            var info = await DownloadHelper.GetInfo(url);
            var file = await DownloadHelper.Download(url, FileName);
            _count++;

            info.File = file;

            _queue.Enqueue(info);

            var audioThread = new Thread(async () =>
            {
                while (_queue.Any() || _adding)
                {
                    try
                    {
                        if (_queue.Any())
                        {
                            var success = _queue.TryPeek(out AudioInfo audioItem);

                            if (!string.IsNullOrWhiteSpace(audioItem.File) && success)
                            {
                                await SendAudio(audioItem.File);
                                File.Delete(audioItem.File);
                                _skip = false;
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (_queue.Any())
                            _queue.TryDequeue(out AudioInfo audioItem);
                    }
                }

                await Stop();
            });

            _status = AudioStatusEnum.Playing;
            audioThread.Start();

            var loadOthersThread = new Thread(async () =>
            {
                if (items.Count > 1)
                {
                    var otheritems = items.GetRange(1, items.Count - 1);
                    await Add(otheritems);
                }
            });

            loadOthersThread.Start();

            return info;
        }

        public async Task Add(List<string> items)
        {
            _adding = true;

            foreach (var url in items)
            {
                if (_status != AudioStatusEnum.Playing)
                {
                    _adding = false;
                    Dispose();
                    return;
                }

                var info = await DownloadHelper.GetInfo(url);
                var file = await DownloadHelper.Download(url, FileName);
                _count++;

                info.File = file;

                _queue.Enqueue(info);
            }

            _adding = false;
        }

        public void PauseAudio()
        {
            Pause = true;
            _status = AudioStatusEnum.Paused;
        }

        public void Resume()
        {
            Pause = false;
            _status = AudioStatusEnum.Playing;
        }

        public async Task Stop()
        {
            Dispose();
            await _currentAudioChannel.StopAsync();
            _status = AudioStatusEnum.Stopped;
        }

        public AudioInfo SkipAudio()
        {
            if (_adding && _queue.Count <= 1)
                return new AudioInfo() { Title = "Loading" };

            _skip = true;

            var nextItem = _queue.ElementAtOrDefault(1);

            if (_queue.Count > 1 && nextItem != null)
                return nextItem;

            return null;
        }

        public AudioInfo CurrentAudio()
        {
            if (_queue.TryPeek(out AudioInfo result))
                return result;

            return new AudioInfo()
            {
                Title = "No audio found",
                Url = "",
                Description = "",
                Duration = ""
            };
        }

        #region Private

        // Initial send audio was based on: mrousavy https://github.com/mrousavy/DiscordMusicBot
        private async Task SendAudio(string filePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-xerror -i \"{filePath}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true
                };

                var ffmpeg = Process.Start(startInfo);

                using (Stream output = ffmpeg.StandardOutput.BaseStream)
                {
                    using (AudioOutStream audioOutput = _currentAudioChannel.CreatePCMStream(AudioApplication.Mixed, null, 1920))
                    {
                        int bufferSize = 3840;
                        int bytesSent = 0;
                        var exit = false;
                        var buffer = new byte[bufferSize];

                        while (!_skip &&
                               !_disposeToken.IsCancellationRequested &&
                               !exit)
                        {
                            try
                            {
                                int read = await output.ReadAsync(buffer, 0, bufferSize, _disposeToken.Token);
                                if (read == 0)
                                {
                                    // End of audio
                                    exit = true;
                                    break;
                                }

                                // Adjust audio levels
                                buffer = ScaleVolumeSafeAllocateBuffers(buffer, _volume);

                                await audioOutput.WriteAsync(buffer, 0, read, _disposeToken.Token);

                                if (Pause)
                                {
                                    bool pauseAgain;

                                    do
                                    {
                                        pauseAgain = await _tcs.Task;
                                        _tcs = new TaskCompletionSource<bool>();
                                    } while (pauseAgain);
                                }

                                bytesSent += read;
                            }
                            catch
                            {
                                exit = true;
                            }
                        }
                        output.Dispose();
                        await audioOutput.FlushAsync();
                    }
                }
            }
            catch
            {
                await Stop();
            }
        }

        private void Dispose()
        {
            _disposeToken.Cancel();

            var disposeThread = new Thread(() =>
            {
                foreach (var song in _queue)
                {
                    try
                    {
                        File.Delete(song.File);
                    }
                    catch { }
                }

                while (_count > 0)
                {
                    try
                    {
                        File.Delete(FileName);
                    }
                    catch { }

                    _count--;
                }
            });

            disposeThread.Start();
        }

        // Source: https://github.com/RogueException/Discord.Net/issues/293
        private byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            if (audioSamples == null) throw new ArgumentException(nameof(audioSamples));
            if (audioSamples.Length % 2 != 0) throw new Exception("Not devisable by 2 (bit)");
            if (volume < 0f || volume > 1f) throw new Exception("Invalid volume");

            var output = new byte[audioSamples.Length];
            if (Math.Abs(volume - 1f) < 0.0001f)
            {
                Buffer.BlockCopy(audioSamples, 0, output, 0, audioSamples.Length);
                return output;
            }

            // 16-bit precision for the multiplication
            int volumeFixed = (int)Math.Round(volume * 65536d);

            for (var i = 0; i < output.Length; i += 2)
            {
                // The cast to short is necessary to get a sign-extending conversion
                int sample = (short)((audioSamples[i + 1] << 8) | audioSamples[i]);
                int processed = (sample * volumeFixed) >> 16;

                output[i] = (byte)processed;
                output[i + 1] = (byte)(processed >> 8);
            }

            return output;
        }

        #endregion
    }
}
