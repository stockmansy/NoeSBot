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
using NoeSbot.Database.Services;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using NoeSbot.Models;
using NoeSbot.Resources;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Audio)]
    public class AudioModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IConfigurationService _service;
        private IMemoryCache _cache;
        private readonly IHttpService _httpService;
        private static ConcurrentDictionary<ulong, AudioPlayer> _currentAudioClients = new ConcurrentDictionary<ulong, AudioPlayer>();
        private IVoiceChannel _currentChannel;
        private readonly GlobalConfig _globalConfig;
        private readonly NotifyLogic _notifyLogic; // TODO move this stuff to another thing

        #region Constructor

        public AudioModule(DiscordSocketClient client, IConfigurationService service, IMemoryCache memoryCache, IHttpService httpService, GlobalConfig globalConfig, NotifyLogic notifyLogic)
        {
            _client = client;
            _service = service;
            _cache = memoryCache;
            _httpService = httpService;
            _globalConfig = globalConfig;
            _notifyLogic = notifyLogic;
        }

        #endregion

        #region Handlers



        #endregion

        #region Commands

        #region AudioInfo

        [Command(Labels.Audio_AudioInfo_Command)]
        [Alias(Labels.Audio_AudioInfo_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetInfo()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Audio_AudioInfo_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Audio_AudioInfo_Command)]
        [Alias(Labels.Audio_AudioInfo_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetInfo(string url)
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

        #endregion

        #region Volume

        [Command(Labels.Audio_Volume_Command)]
        [Alias(Labels.Audio_Volume_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SetAudio()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Audio_Volume_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Audio_Volume_Command)]
        [Alias(Labels.Audio_Volume_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SetAudio(int volume)
        {
            if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
            {
                audioplayer.SetVolume(volume);
            }

            var success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.AudioVolume, volume.ToString());

            await _globalConfig.LoadInGuildConfigs();

            if (success)
                await ReplyAsync($"Changed the audio level to: {volume}");
            else
                await ReplyAsync("Failed to change the volume level");
        }

        #endregion

        #region Play

        [Command(Labels.Audio_Play_Command)]
        [Alias(Labels.Audio_Play_Alias_1, Labels.Audio_Play_Alias_2, Labels.Audio_Play_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [RequireBotPermission(GuildPermission.Connect)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Play()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Audio_Play_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Audio_Play_Command)]
        [Alias(Labels.Audio_Play_Alias_1, Labels.Audio_Play_Alias_2, Labels.Audio_Play_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [RequireBotPermission(GuildPermission.Connect)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Play([Remainder] string input)
        {
            try
            {
                var user = Context.User as SocketGuildUser;
                _currentChannel = user.VoiceChannel;
                var info = new AudioInfo();

                var url = input;
                if (!DownloadHelper.IsValidUrl(url))
                    url = await _notifyLogic.GetYoutubeLink(input);

                if (!DownloadHelper.IsValidUrl(url)) // Todo do this a bit more elegantly
                    throw new Exception("Invalid Url!");

                if (_currentChannel != null)
                {
                    var message = await ReplyAsync("", false, GetLoadingEmbed(url, user));
                    var audioThread = new Thread(async () =>
                    {
                        try
                        {
                            var items = await DownloadHelper.GetItems(url);
                            if (items.Count < 1)
                                items = await DownloadHelper.GetItems(url, true);
                            if (items.Count < 1)
                                throw new Exception("No items found");

                            var textChannel = await Context.Guild.GetDefaultChannelAsync();

                            if (!_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
                            {
                                // Create new audioplayer
                                audioplayer = new AudioPlayer(_currentChannel, textChannel, Context.Guild.Id, GlobalConfig.GetGuildConfig(Context.Guild.Id).AudioVolume);
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
                                audioplayer = new AudioPlayer(_currentChannel, textChannel, Context.Guild.Id, GlobalConfig.GetGuildConfig(Context.Guild.Id).AudioVolume);
                                _currentAudioClients.TryAdd(Context.Guild.Id, audioplayer);
                                info = await audioplayer.Start(items);
                            }
                            else
                            {
                                // Add audio items to the queue
                                await audioplayer.Add(items);
                                info = audioplayer.CurrentAudio();
                            }

                            await message?.ModifyAsync(x => x.Embed = GetCurrentAudioEmbed(info, user));
                        }
                        catch
                        {
                            await message?.DeleteAsync();
                            await ReplyAsync("", false, GetFailedEmbed(user));
                        }
                    });

                    audioThread.Start();
                }
                else
                {
                    await ReplyAsync("", false, GetNotInChannelEmbed(url, user));
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        #endregion

        #region Pause

        [Command(Labels.Audio_Pause_Command)]
        [Alias(Labels.Audio_Pause_Alias_1, Labels.Audio_Pause_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Pause()
        {
            if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
            {
                audioplayer.PauseAudio();
                
                await ReplyAsync("Paused the audio");
            } else
                await ReplyAsync("Audio not playing");


        }

        #endregion

        #region Resume

        [Command(Labels.Audio_Resume_Command)]
        [Alias(Labels.Audio_Resume_Alias_1, Labels.Audio_Resume_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Resume()
        {
            if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
            {
                audioplayer.Resume();

                await ReplyAsync("Resumed playing the audio");
            }
            else
                await ReplyAsync("Audio not playing");            
        }

        #endregion

        #region Stop

        [Command(Labels.Audio_Stop_Command)]
        [Alias(Labels.Audio_Stop_Alias_1, Labels.Audio_Stop_Alias_2, Labels.Audio_Stop_Alias_3)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Stop()
        {
            if (_currentAudioClients.TryGetValue(Context.Guild.Id, out AudioPlayer audioplayer))
            {
                await audioplayer.Stop();
                _currentAudioClients.TryRemove(Context.Guild.Id, out AudioPlayer removedPlayer);
            }

            await ReplyAsync("Stopped playing the audio");
        }

        #endregion

        #region Skip

        [Command(Labels.Audio_Skip_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Skip()
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

        #endregion

        #region Current

        [Command(Labels.Audio_Current_Command)]
        [Alias(Labels.Audio_Current_Alias_1, Labels.Audio_Current_Alias_2)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task CurrentAudio()
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

        #endregion

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

        private Embed GetNotInChannelEmbed(string url, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You aren't in any audio channel"
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

        private Embed GetFailedEmbed(SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "Something went wrong when trying to play the requested audio"
            };

            builder.AddField(x =>
            {
                x.Name = "Bugreport";
                x.Value = "Please let me know how you broke it at https://github.com/stockmansy/NoeSBot";
                x.IsInline = false;
            });

            return builder.Build();
        }

        #endregion
    }
}
