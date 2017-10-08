using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using Discord;
using System.Threading.Tasks;
using NoeSbot.Helpers;
using System;
using NoeSbot.Database.Services;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoeSbot.Models;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Notify)]
    public class NotifyModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly INotifyService _notifyService;
        private readonly IHttpService _httpService;
        private IMemoryCache _cache;

        #region Constructor

        public NotifyModule(DiscordSocketClient client, INotifyService notifyService, IHttpService httpService, IMemoryCache memoryCache)
        {
            _client = client;
            _notifyService = notifyService;
            _httpService = httpService;
            _cache = memoryCache;
        }

        #endregion

        #region Help

        [Command("addstream")]
        [Summary("Add a stream")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task Nuke()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "This will nuke all messages send to the current channel. (Max 1 hour)"
                };

                builder.AddField(x =>
                {
                    x.Name = "What will happen?";
                    x.Value = $"UserX put this channel in nuke mode.{Environment.NewLine}You will not be able to send any messages unless you are a server admin.";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Example";
                    x.Value = "~nuke 2m";
                    x.IsInline = false;
                });

                await ReplyAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Commands

        [Command("addstream")]
        [Summary("Add a stream by username")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task AddStream(string type, string name)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                switch (type.ToLower())
                {
                    case "twitch":
                        await AddTwitchStream(name);
                        break;
                }
            }
        }

        [Command("addtwitchstream")]
        [Summary("Add a twitch stream by username")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task AddTwitchStream(string name)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                var content = await _httpService.SendTwitch(HttpMethod.Get, $"https://api.twitch.tv/kraken/users?login={name}", "e0ggcjd1zomziofv6qrsa5gn1hf6d4");
                var response = await content.ReadAsStringAsync();
                var root = JsonConvert.DeserializeObject<TwitchUsersRoot>(response);

                if (root.Total > 0)
                {
                    var twitchUser = root.Users[0];
                    var builder = new EmbedBuilder()
                    {
                        Color = user.GetColor()
                    };

                    var twitchName = (twitchUser.DisplayName.Equals(twitchUser.Name)) ? twitchUser.DisplayName : $"{twitchUser.DisplayName} ({twitchUser.Name})";

                    var success = await _notifyService.AddNotifyItem((long)Context.Guild.Id, (long)user.Id, twitchName, twitchUser.Id, twitchUser.Logo, (int)NotifyEnum.Twitch);

                    if (success)
                    {
                        builder.AddField(x =>
                        {
                            x.Name = "Added the stream";
                            x.Value = $"{user.Username} added a stream for twitch user {twitchName}";
                            x.IsInline = false;
                        });
                    }
                    else
                    {
                        builder.AddField(x =>
                        {
                            x.Name = "Failed to add the stream";
                            x.Value = $"{user.Username} failed to add a stream for twitch user {twitchName}";
                            x.IsInline = false;
                        });
                    }

                    if (twitchUser.Logo != null)
                        builder.WithThumbnailUrl(twitchUser.Logo);

                    await ReplyAsync("", false, builder.Build());
                }
            }
        }

        #endregion

        #region Private



        #endregion
    }
}
