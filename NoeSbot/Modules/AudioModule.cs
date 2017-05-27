﻿using Discord.Commands;
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
using NoeSbot.Database.Services;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using NoeSbot.Models;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Audio)]
    public class AudioModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IConfigurationService _service;
        private IMemoryCache _cache;
        private static ConcurrentDictionary<ulong, AudioPlayer> _currentAudioClients = new ConcurrentDictionary<ulong, AudioPlayer>();
        private IVoiceChannel _currentChannel;

        #region Constructor

        public AudioModule(DiscordSocketClient client, IConfigurationService service, IMemoryCache memoryCache)
        {
            _client = client;
            _service = service;
            _cache = memoryCache;
        }

        #endregion

        #region Handlers



        #endregion

        #region Help text

        [Command("audioinfo")]
        [Alias("musicinfo")]
        [Summary("Get info about an audio item")]
        [MinPermissions(AccessLevel.User)]
        public async Task GetInfoHelp()
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You can get info about an audio item:"
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter: The url";
                x.Value = "Provide an url to the audio item";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("volume")]
        [Alias("v")]
        [Summary("Set the audio level")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task SetAudioHelp()
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You can set the volume level:"
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter: a number";
                x.Value = "Provide a number between 1 and 10";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("play")]
        [Alias("p", "playaudio", "playsong")]
        [Summary("Start playing audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task PlayHelp()
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You can play an audio item:"
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter: the url";
                x.Value = "Provide an url to the audio item";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Commands

        [Command("audioinfo")]
        [Alias("musicinfo")]
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

        [Command("volume")]
        [Alias("v")]
        [Summary("Set the audio level")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task SetAudio(int volume)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                {
                    audioplayer.SetVolume(volume);
                }

                var success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.GeneralChannel, volume.ToString());

                await Configuration.LoadAsync(_service);

                if (success)
                    await ReplyAsync($"Changed the audio level to: {volume}");
                else
                    await ReplyAsync("Failed to change the volume level");
            }
        }

        [Command("play")]
        [Alias("p", "playaudio", "playsong")]
        [Summary("Start playing audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task Play([Remainder] string url)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                try
                {
                    var user = Context.User as SocketGuildUser;
                    _currentChannel = user.VoiceChannel;
                    var info = new AudioInfo();

                    var message = await ReplyAsync("", false, GetLoadingEmbed(url, user));

                    if (_currentChannel != null)
                    {
                        var audioThread = new Thread(async () =>
                        {
                            var items = await DownloadHelper.GetItems(url);
                            if (items.Count < 1) return;

                            var textChannel = await Context.Guild.GetDefaultChannelAsync();

                            if (!_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                            {
                                // Create new audioplayer
                                audioplayer = new AudioPlayer(_currentChannel, textChannel, Context.Guild.Id, Configuration.Load(Context.Guild.Id).AudioVolume);
                                _currentAudioClients.TryAdd(Context.Guild.Id, audioplayer);
                                info = await audioplayer.Start(items);
                            }
                            else if (audioplayer.CurrentVoiceChannel != _currentChannel.Id || audioplayer.Status == AudioStatusEnum.Stopped)
                            {
                                // Stop existing player
                                if (audioplayer.Status != AudioStatusEnum.Stopped)
                                    await audioplayer.Stop();
                                _currentAudioClients.TryRemove(Context.Guild.Id, out AudioPlayer removedPlayer);

                                // Create new audioplayer
                                audioplayer = new AudioPlayer(_currentChannel, textChannel, Context.Guild.Id, Configuration.Load(Context.Guild.Id).AudioVolume);
                                _currentAudioClients.TryAdd(Context.Guild.Id, audioplayer);
                                info = await audioplayer.Start(items);
                            }
                            else
                            {
                                // Add audio items to the queue
                                await audioplayer.Add(items);
                                info = audioplayer.CurrentAudio();
                            }

                            try
                            {
                                await message.ModifyAsync(x => x.Embed = GetCurrentAudioEmbed(info, user));
                            }
                            catch
                            {
                                await ReplyAsync("", false, GetLoadingEmbed(url, user));
                            }
                        });

                        audioThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }
        }

        [Command("stop")]
        [Alias("s", "stopaudio", "stopsong")]
        [Summary("Stop playing audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task Stop()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                {
                    await audioplayer.Stop();
                    _currentAudioClients.TryRemove(Context.Guild.Id, out AudioPlayer removedPlayer);
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
                    var info = audioplayer.SkipAudio();
                    if (info != null)
                    {
                        var user = Context.User as SocketGuildUser;

                        if (info.Title.Equals("loading", StringComparison.OrdinalIgnoreCase))
                            await ReplyAsync("The next song is still loading...");
                        else
                            await ReplyAsync("", false, GetCurrentAudioEmbed(info, user));
                    }
                    else
                        await ReplyAsync("That was the last song");
                }
            }
        }

        [Command("current")]
        [Alias("currentaudio", "currentsong")]
        [Summary("Get the current audio")]
        [MinPermissions(AccessLevel.User)]
        public async Task CurrentAudio()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                {
                    var user = Context.User as SocketGuildUser;
                    var info = audioplayer.CurrentAudio();
                    await ReplyAsync("", false, GetCurrentAudioEmbed(info, user));
                }
                else
                    await ReplyAsync("No audio playing currently");
            }
        }

        #endregion

        #region Private

        private Embed GetLoadingEmbed(string url, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "Loading the requested audio..."
            };

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = url;
                x.IsInline = false;
            });

            return builder.Build();
        }

        private Embed GetCurrentAudioEmbed(AudioInfo currentAudio, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "Now playing (Keep in mind, playlists have to be buffered, also WIP ;)):"
            };

            builder.AddField(x =>
            {
                x.Name = "Title";
                x.Value = currentAudio?.Title ?? "No title found";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = currentAudio?.Url ?? "No url found";
                x.IsInline = false;
            });

            return builder.Build();
        }

        #endregion
    }
}