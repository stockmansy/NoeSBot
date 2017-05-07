using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using System.Threading.Tasks;
using Discord;
using NoeSbot.Helpers;
using Discord.Audio;
using System;
using NoeSbot.Logic;
using System.Collections.Generic;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Music)]
    public class AudioModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private static Dictionary<ulong, AudioPlayer> _currentAudioClients = new Dictionary<ulong, AudioPlayer>();
        private IVoiceChannel _currentChannel;

        #region Constructor

        public AudioModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        #endregion

        #region Handlers



        #endregion

        #region Commands

        [Command("musicinfo")]
        [Alias("mi")]
        [Summary("Get info about a video")]
        [MinPermissions(AccessLevel.User)]
        public async Task GetInfo(string url)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                var info = await DownloadHelper.GetInfo(url);

                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = ""
                };

                if (!string.IsNullOrWhiteSpace(info.Title))
                {
                    builder.AddField(x =>
                    {
                        x.Name = "Title";
                        x.Value = info.Title;
                        x.IsInline = false;
                    });
                }

                if (!string.IsNullOrWhiteSpace(info.Description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = "Video Description";
                        x.Value = info.Description;
                        x.IsInline = false;
                    });
                }

                if (!string.IsNullOrWhiteSpace(info.Duration))
                {
                    builder.AddField(x =>
                    {
                        x.Name = "Duration";
                        x.Value = info.Duration;
                        x.IsInline = false;
                    });
                }

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("play")]
        [Alias("p")]
        [Summary("Start playing audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task Play(string url)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                try
                {
                    var user = Context.User as SocketGuildUser;
                    _currentChannel = user.VoiceChannel;

                    if (_currentChannel != null)
                    {
                        var textChannel = await Context.Guild.GetDefaultChannelAsync();

                        if (!_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                        {
                            audioplayer = new AudioPlayer(_currentChannel, textChannel, Context.Guild.Id);
                            _currentAudioClients.Add(Context.Guild.Id, audioplayer);
                            await audioplayer.Start(url);
                        } else if (audioplayer.CurrentVoiceChannel != _currentChannel.Id)
                        {
                            await audioplayer.Stop();
                            _currentAudioClients.Remove(Context.Guild.Id);
                            audioplayer = new AudioPlayer(_currentChannel, textChannel, Context.Guild.Id);
                            _currentAudioClients.Add(Context.Guild.Id, audioplayer);
                            await audioplayer.Start(url);
                        } else
                        {
                            await audioplayer.Add(url);
                        }
                    }

                    var builder = new EmbedBuilder()
                    {
                        Color = user.GetColor(),
                        Description = "You can start playing a sound clip:"
                    };

                    builder.AddField(x =>
                    {
                        x.Name = "Parameter 1: The subject";
                        x.Value = "Ignore this, is in development";
                        x.IsInline = false;
                    });

                    await ReplyAsync("", false, builder.Build());
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }
        }

        [Command("stop")]
        [Alias("s")]
        [Summary("Stop playing audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task Stop()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                {
                    await audioplayer.Stop();
                    _currentAudioClients.Remove(Context.Guild.Id);
                }

                await ReplyAsync("Stopped playing the audio");
            }
        }

        [Command("skip")]
        [Summary("Skip some audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task Skip()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                {
                    audioplayer.SkipAudio();
                }

                await ReplyAsync("Skipped some audio");
            }
        }

        #endregion

        #region Private

        // Should probably do this in a better different way
        internal static async Task AudioDoneAsync(ulong guildId)
        {
            if (_currentAudioClients.TryGetValue(guildId, out AudioPlayer audioplayer))
            {
                await audioplayer.Stop();
                _currentAudioClients.Remove(guildId);
            }
        }

        #endregion
    }
}
