using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoeSbot.Converters;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Extensions;
using NoeSbot.Helpers;
using NoeSbot.Models;
using NoeSbot.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class NotifyLogic
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationService _configurationService;
        private readonly INotifyService _notifyService;
        private readonly IHttpService _httpService;
        private ConcurrentDictionary<int, DateTime?> _printed;
        private readonly LimitedDictionary<ulong, NotifyItem> _notifyMessages;
        private readonly IMemoryCache _cache;

        public NotifyLogic(DiscordSocketClient client, IConfigurationService configurationService, INotifyService notifyService, IHttpService httpService, IMemoryCache memoryCache)
        {
            _client = client;
            _configurationService = configurationService;
            _httpService = httpService;
            _notifyService = notifyService;
            _printed = new ConcurrentDictionary<int, DateTime?>();
            _notifyMessages = new LimitedDictionary<ulong, NotifyItem>();
            _cache = memoryCache;

            client.ReactionAdded += OnReactionAdded;
        }

        protected async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null || !reaction.User.IsSpecified)
                return;
                        
            var userAdjusting = reaction.User.Value;

            if (!userAdjusting.IsBot && !userAdjusting.IsWebhook)
            {
                var success = _notifyMessages.TryGetValue(message.Id, out NotifyItem notifyItem);
                if (!success)
                    return;

                var name = reaction.Emote.Name;

                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                
                new Thread(async () =>
                {
                    if (name.Equals(IconHelper.Bell))
                    {
                        var addSuccess = await _notifyService.AddUserToNotifyItem((long)userAdjusting.Id, notifyItem.Id);
                        if (addSuccess)
                            await userAdjusting.SendMessageAsync("Successfully added you to the subscription list");
                        else
                            await userAdjusting.SendMessageAsync("Something went wrong trying to add you to the subscription list");
                    }
                    else if (name.Equals(IconHelper.BellStop))
                    {
                        var removeSuccess = await _notifyService.RemoveUserFromNotifyItem((long)userAdjusting.Id, notifyItem.Id);
                        if (removeSuccess)
                            await userAdjusting.SendMessageAsync("Successfully removed you from the subscription list");
                        else
                            await userAdjusting.SendMessageAsync("Something went wrong trying to remove you to the subscription list");
                        
                    }
                }).Start();
            }
        }

        public async Task Run(CancellationToken cancelToken)
        {
            try
            {
                var initial = DateTime.Now;

                while (!cancelToken.IsCancellationRequested)
                {
                    var notifyItems = await _notifyService.RetrieveAllNotifysAsync();
                    foreach (var notifyItem in notifyItems)
                    {
                        // Don't spam every stream on startup
                        if (_printed.Count <= 0 && initial.AddSeconds(10) > DateTime.Now)
                        {
                            _printed.AddOrUpdate(notifyItem.NotifyItemId, DateTime.Now);
                            continue;
                        }

                        var guild = _client.GetGuild((ulong)notifyItem.GuildId) as IGuild;

                        if (!Configuration.Load(guild.Id).LoadedModules.Contains((int)ModuleEnum.Notify))
                            return;

                        switch (notifyItem.Type)
                        {
                            case (int)NotifyEnum.Twitch:
                                try
                                {
                                    var content = await _httpService.SendTwitch(HttpMethod.Get, $"https://api.twitch.tv/kraken/streams/{notifyItem.Value}", Configuration.Load().TwitchClientId);
                                    var response = await content.ReadAsStringAsync();
                                    var root = JsonConvert.DeserializeObject<TwitchStreamRoot>(response, new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore,
                                        Formatting = Formatting.None,
                                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                        Converters = new List<JsonConverter> { new DecimalConverter() }
                                    });

                                    // Try to get the existing item
                                    var existing = _printed.TryGetValue(notifyItem.NotifyItemId, out DateTime? printedItem);

                                    // Is the stream online?
                                    if (root.Stream != null)
                                    {
                                        if (existing && (printedItem == null || (printedItem.HasValue && printedItem.Value.AddHours(6) < DateTime.Now))) // Initial print and new print every hour
                                        {
                                            _printed.AddOrUpdate(notifyItem.NotifyItemId, DateTime.Now);

                                            var msg = await GetMessage(notifyItem, guild);
                                            await PrintNotification(msg, root, guild, notifyItem.NotifyItemId);
                                            continue;
                                        }
                                        else if (!existing)
                                        {
                                            // New stream added
                                            _printed.AddOrUpdate(notifyItem.NotifyItemId, DateTime.Now);

                                            var msg = await GetMessage(notifyItem, guild);
                                            await PrintNotification(msg, root, guild, notifyItem.NotifyItemId);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // To avoid printing too many times in case of stream problems
                                        var oldMsg = await CheckForOlderMessages(guild);
                                        if ((existing && printedItem.HasValue && printedItem.Value.AddMinutes(30) < DateTime.Now) || oldMsg)
                                            continue;

                                        // Set item as offline
                                        _printed.AddOrUpdate(notifyItem.NotifyItemId, null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(5000);
                                    continue;
                                }
                                break;
                                case (int)NotifyEnum.Youtube:
                                try
                                {
                                    var content = await _httpService.Send(HttpMethod.Get, $"https://www.googleapis.com/youtube/v3/search?part=snippet&channelId={notifyItem.Value}&type=video&eventType=live&key={Configuration.Load().YoutubeApiKey}");
                                    var response = await content.ReadAsStringAsync();
                                    var root = JsonConvert.DeserializeObject<YoutubeStream>(response, new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore,
                                        Formatting = Formatting.None,
                                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                        Converters = new List<JsonConverter> { new DecimalConverter() }
                                    });

                                    var existing = _printed.TryGetValue(notifyItem.NotifyItemId, out DateTime? printedItem);

                                    if (root.Items != null && root.Items.Length > 0)
                                    {
                                        var item = root.Items[0];
                                        
                                        if (existing && (printedItem == null || (printedItem.HasValue && printedItem.Value.AddHours(6) < DateTime.Now))) // Initial print and new print every hour
                                        {
                                            _printed.AddOrUpdate(notifyItem.NotifyItemId, DateTime.Now);

                                            var msg = await GetMessage(notifyItem, guild);
                                            await PrintNotification(msg, item, guild, notifyItem.NotifyItemId);
                                            continue;
                                        }
                                        else if (!existing)
                                        {
                                            // New stream added
                                            _printed.AddOrUpdate(notifyItem.NotifyItemId, DateTime.Now);

                                            var msg = await GetMessage(notifyItem, guild);
                                            await PrintNotification(msg, item, guild, notifyItem.NotifyItemId);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // To avoid printing too many times in case of stream problems
                                        var oldMsg = await CheckForOlderMessages(guild);
                                        if ((existing && printedItem.HasValue && printedItem.Value.AddMinutes(30) < DateTime.Now) || oldMsg)
                                            continue;

                                        // Set item as offline
                                        _printed.AddOrUpdate(notifyItem.NotifyItemId, null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await Task.Delay(5000);
                                    continue;
                                }
                                break;
                        }
                    }

                    await Task.Delay(1000 * 60, cancelToken); //Wait 60 seconds between full updates
                }
            }
            catch (TaskCanceledException) { }
        }

        #region Private

        private async Task<string> GetMessage(Database.Models.NotifyItem notifyItem, IGuild guild)
        {
            var msg = "";
            foreach (var u in notifyItem.Users)
            {
                var us = await guild.GetUserAsync((ulong)u.UserId);
                msg += $"{us.Mention}, ";
            }

            foreach (var r in notifyItem.Roles)
            {
                var ro = guild.GetRole((ulong)r.RoleId);
                msg += $"{ro.Mention}, ";
            }

            if (!string.IsNullOrWhiteSpace(msg))
                msg = msg.Substring(0, msg.Length - 2);

            return msg;
        }

        private async Task PrintNotification(string mentions, TwitchStreamRoot root, IGuild guild, int notifyItemId)
        {
            var stream = root.Stream;

            var builder = new EmbedBuilder()
            {
                Color = Color.Red,
                Url = root.Stream.Channel.Url
            };

            var twitchName = (stream.Channel.DisplayName.Equals(stream.Channel.Name)) ? stream.Channel.DisplayName : $"{stream.Channel.DisplayName} ({stream.Channel.Name})";

            builder.AddField(x =>
            {
                x.Name = $"{twitchName}";
                x.Value = $"twitch user {twitchName} started streaming{Environment.NewLine}{Environment.NewLine}Game: {stream.Game}          Current viewers: {stream.ViewerCount}{Environment.NewLine}{Environment.NewLine}";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = root.Stream.Channel.Url;
                x.IsInline = false;
            });

            if (stream.Channel.Logo != null)
                builder.WithThumbnailUrl(stream.Channel.Logo);

            var defaultChannel = await guild.GetDefaultChannelAsync();
            var message = await defaultChannel.SendMessageAsync(mentions, false, builder.Build());

            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.Bell));
            await Task.Delay(1250);
            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.BellStop));

            _notifyMessages.Add(message.Id, new NotifyItem { Id = notifyItemId, Type = NotifyEnum.Twitch });
        }

        private async Task PrintNotification(string mentions, YoutubeStream.YoutubeVideoItem item, IGuild guild, int notifyItemId)
        {
            var url = $"https://youtu.be/{item.Id.VideoId}"; // Todo find out url for .com
            var builder = new EmbedBuilder()
            {
                Color = Color.Red,
                Url = url
            };

            var youtubeName = CommonHelper.FirstLetterToUpper(item.Snippet.ChannelTitle);

            builder.AddField(x =>
            {
                x.Name = $"{youtubeName}";
                x.Value = $"youtube channel {youtubeName} started streaming{Environment.NewLine}{Environment.NewLine}";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {                
                x.Name = "Url";
                x.Value = url;
                x.IsInline = false;
            });
            
            if (item.Snippet.Thumbnails?.Default != null)
                builder.WithThumbnailUrl(item.Snippet.Thumbnails.Default.Url);

            var defaultChannel = await guild.GetDefaultChannelAsync();
            var message = await defaultChannel.SendMessageAsync(mentions, false, builder.Build());

            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.Bell));
            await Task.Delay(1250);
            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.BellStop));

            _notifyMessages.Add(message.Id, new NotifyItem { Id = notifyItemId, Type = NotifyEnum.Youtube });
        }

        private async Task<bool> CheckForOlderMessages(IGuild guild)
        {
            var oldMsgs = await GetLastFewMessages(guild);
            return oldMsgs.Where(msg => msg.Embeds.FirstOrDefault()?.Fields.FirstOrDefault(f => f.Value.ToLowerInvariant().Contains("started streaming")) != null && msg.Author.IsBot).Any();
        }

        private async Task<IEnumerable<IMessage>> GetLastFewMessages(IGuild guild)
        {
            if (!_cache.TryGetValue(CacheEnum.NotifyMessages, out IEnumerable<IMessage> cacheEntry))
            {
                var defaultChannel = await guild.GetDefaultChannelAsync();
                cacheEntry = await defaultChannel.GetMessagesAsync(15, options: new RequestOptions
                {
                    AuditLogReason = "Checking if people were notified"
                }).Flatten();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(CacheEnum.NotifyMessages, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        #endregion
    }
}
