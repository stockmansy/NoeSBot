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

        public NotifyLogic(DiscordSocketClient client, IConfigurationService configurationService, INotifyService notifyService, IHttpService httpService)
        {
            _client = client;
            _configurationService = configurationService;
            _httpService = httpService;
            _notifyService = notifyService;
            _printed = new ConcurrentDictionary<int, DateTime?>();
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
                        var guild = _client.GetGuild((ulong)notifyItem.GuildId) as IGuild;
                        switch (notifyItem.Type)
                        {
                            case (int)NotifyEnum.Twitch:
                                try
                                {
                                    var content = await _httpService.SendTwitch(HttpMethod.Get, $"https://api.twitch.tv/kraken/streams/{notifyItem.Value}", "e0ggcjd1zomziofv6qrsa5gn1hf6d4");
                                    var response = await content.ReadAsStringAsync();
                                    var root = JsonConvert.DeserializeObject<TwitchStreamRoot>(response, new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore,
                                        Formatting = Formatting.None,
                                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                        Converters = new List<JsonConverter> { new DecimalConverter() }
                                    });

                                    if (root.Stream != null)
                                    {
                                        var existing = _printed.TryGetValue(notifyItem.NotifyItemId, out DateTime? printedItem);
                                        if (existing && printedItem.HasValue && printedItem.Value.AddHours(1) < DateTime.Now)
                                        {
                                            var msg = await GetMessage(notifyItem, guild);
                                            await PrintNotification(msg, root, guild);
                                            continue;
                                        }
                                        else if (!existing)
                                        {
                                            _printed.AddOrUpdate(notifyItem.NotifyItemId, DateTime.Now);

                                            if (initial.AddMinutes(5) < DateTime.Now)
                                            {
                                                var msg = await GetMessage(notifyItem, guild);
                                                await PrintNotification(msg, root, guild);
                                            }
                                        }
                                    }
                                    else
                                    {
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

        private async Task PrintNotification(string mentions, TwitchStreamRoot root, IGuild guild)
        {
            var stream = root.Stream;

            var builder = new EmbedBuilder()
            {
                Color = Color.Red
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
            await defaultChannel.SendMessageAsync(mentions, false, builder.Build());
        }

        #endregion
    }
}
