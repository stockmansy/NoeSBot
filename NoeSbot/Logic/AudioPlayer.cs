using Discord;
using Discord.Audio;
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
        private Queue<AudioInfo> _queue;
        private bool _internalSkip;
        private ulong _guildId;

        public AudioPlayer (IVoiceChannel voiceChannel, ITextChannel textChannel, ulong guildId)
        {
            _voiceChannel = voiceChannel ?? throw new ArgumentNullException(nameof(voiceChannel));
            _textChannel = textChannel ?? throw new ArgumentNullException(nameof(textChannel));
            _disposeToken = new CancellationTokenSource();
            _queue = new Queue<AudioInfo>();
            _internalSkip = false;
            _guildId = guildId;
        }

        public ulong CurrentVoiceChannel => _voiceChannel.Id;

        private bool Skip
        {
            get
            {
                var result = _internalSkip;
                _internalSkip = false;
                return result;
            }
            set => _internalSkip = value;
        }        

        public async Task Start(string url)
        {
            var audioThread = new Thread(async () =>
            {
                _currentAudioChannel = await _voiceChannel.ConnectAsync();

                var info = await DownloadHelper.GetInfo(url);
                var file = await DownloadHelper.Download(url);

                info.File = file;

                _queue.Enqueue(info);

                while (_queue.Any()) {
                    var audioItem = _queue.Peek();

                    if (!string.IsNullOrWhiteSpace(audioItem.File)) {                         
                        try
                        {
                            await SendAudio(audioItem.File);
                            File.Delete(audioItem.File);
                        }
                        catch { }
                        finally
                        {
                            if (_queue.Any())
                                _queue.Dequeue();
                        }
                    }
                    else { 
                        if (_queue.Any())
                            _queue.Dequeue();
                    }
                }

                await AudioModule.AudioDoneAsync(_guildId);
            });

            audioThread.Start();
            await Task.CompletedTask;
        }

        public async Task Add(string url)
        {
            var info = await DownloadHelper.GetInfo(url);
            var file = await DownloadHelper.Download(url);

            info.File = file;

            _queue.Enqueue(info);
        }

        public async Task Stop()
        {
            Dispose();
            await _currentAudioChannel.StopAsync();
        }

        public void SkipAudio()
        {
            Skip = true;
        }

        #region Private

        // Used the same way to stream the audio as:  mrousavy https://github.com/mrousavy/DiscordMusicBot
        private async Task SendAudio(string filePath)
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
                using (AudioOutStream audioOutput = _currentAudioChannel.CreatePCMStream(AudioApplication.Mixed, 1920))
                {
                    int bufferSize = 3840;
                    int bytesSent = 0;
                    var exit = false;
                    var buffer = new byte[bufferSize];

                    while (!Skip &&
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

                            await audioOutput.WriteAsync(buffer, 0, read, _disposeToken.Token);

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

        // Improve this
        private void Dispose()
        {
            _disposeToken.Cancel();

            var disposeThread = new Thread(() => {
                foreach (var song in _queue)
                {
                    try
                    {
                        File.Delete(song.File);
                    }
                    catch { }
                }

                _queue.Clear();
            });

            disposeThread.Start();        
        }

        #endregion
    }
}
