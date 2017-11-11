using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoeSbot.Converters;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Extensions;
using NoeSbot.Helpers;
using NoeSbot.Models;
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

        public NotifyLogic(DiscordSocketClient client, IConfigurationService configurationService, INotifyService notifyService, IHttpService httpService)
        {
            _client = client;
            _configurationService = configurationService;
            _httpService = httpService;
            _notifyService = notifyService;
            _printed = new ConcurrentDictionary<int, DateTime?>();
            _notifyMessages = new LimitedDictionary<ulong, NotifyItem>();

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

                                    // Is the stream online?
                                    if (root.Stream != null)
                                    {
                                        var existing = _printed.TryGetValue(notifyItem.NotifyItemId, out DateTime? printedItem);
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

                                    if (root.Items != null && root.Items.Length > 0)
                                    {
                                        var item = root.Items[0];
                                        var existing = _printed.TryGetValue(notifyItem.NotifyItemId, out DateTime? printedItem);
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
                Url = root.Stream.Channel.Url,
                Footer = new EmbedFooterBuilder { Text = root.Stream.Channel.Url }
            };

            var twitchName = (stream.Channel.DisplayName.Equals(stream.Channel.Name)) ? stream.Channel.DisplayName : $"{stream.Channel.DisplayName} ({stream.Channel.Name})";

            builder.AddField(x =>
            {
                x.Name = $"{twitchName}";
                x.Value = $"twitch user {twitchName} started streaming{Environment.NewLine}{Environment.NewLine}Game: {stream.Game}          Current viewers: {stream.ViewerCount}";
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
            var url = $"https://www.youtube.com/?v={item.Id.VideoId}";
            var builder = new EmbedBuilder()
            {
                Color = Color.Red,
                Url = url,
                Footer = new EmbedFooterBuilder { Text = url }
            };

            var youtubeName = CommonHelper.FirstLetterToUpper(item.Snippet.ChannelTitle);

            builder.AddField(x =>
            {
                x.Name = $"{youtubeName}";
                x.Value = $"youtube channel {youtubeName} started streaming";
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

        #endregion
    }
}
