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
        public async Task AddStream(string type, string name, string role = "")
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                switch (type.ToLower())
                {
                    case "twitch":
                        await AddTwitchStream(name, role);
                        break;
                }
            }
        }

        [Command("addtwitchstream")]
        [Summary("Add a twitch stream by username")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task AddTwitchStream(string name, string role = "")
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

                    var success = false;
                    if (string.IsNullOrWhiteSpace(role))
                        success = await _notifyService.AddNotifyItem((long)Context.Guild.Id, (long)user.Id, twitchName, twitchUser.Id, twitchUser.Logo, (int)NotifyEnum.Twitch);
                    else
                    {
                        foreach (var r in Context.Guild.Roles)
                        {
                            if (r.Name.Equals(role, StringComparison.OrdinalIgnoreCase))
                            {
                                success = await _notifyService.AddNotifyItemRole((long)Context.Guild.Id, (long)r.Id, twitchName, twitchUser.Id, twitchUser.Logo, (int)NotifyEnum.Twitch);
                                break;
                            }
                        }
                    }

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

        [Command("allstreams")]
        [Alias("getstreams", "streams")]
        [Summary("Get all streams")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task AllStreams()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor()
                };

                var streams = await _notifyService.RetrieveAllNotifysAsync();

                foreach (var stream in streams)
                {
                    var users = "";
                    foreach (var u in stream.Users)
                    {
                        var us = await Context.Guild.GetUserAsync((ulong)u.UserId);
                        users += $"{us.Username}, ";
                    }

                    if (!string.IsNullOrWhiteSpace(users))
                        users = users.Substring(0, users.Length - 2);
                    else users = "/";

                    var roles = "";
                    foreach (var r in stream.Roles)
                    {
                        var ro = Context.Guild.GetRole((ulong)r.RoleId);
                        users += $"{ro.Name}, ";
                    }

                    if (!string.IsNullOrWhiteSpace(roles))
                        roles = roles.Substring(0, roles.Length - 2);
                    else roles = "/";

                    var type = "";
                    switch (stream.Type)
                    {
                        case (int)NotifyEnum.Twitch:
                            type = "Twitch";
                            break;
                        case (int)NotifyEnum.Youtube:
                            type = "Youtube";
                            break;
                    }

                    builder.AddField(x =>
                    {
                        x.Name = $"{stream.Name}";
                        x.Value = $"Users: {users}{Environment.NewLine}Roles: {roles}{Environment.NewLine}Type: {type}";
                        x.IsInline = false;
                    });
                }

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("removetwitchstream")]
        [Alias("deletetwitchstream")]
        [Summary("Remove twitch stream")]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task RemoveTwitchStream(string name)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var success = await _notifyService.RemoveNotifyItem((long)Context.Guild.Id, name, (int)NotifyEnum.Twitch);

                if (success)
                    await ReplyAsync($"Removed the twitch stream {name}");
                else
                    await ReplyAsync($"Failed to remove the twitch stream {name}");
            }
        }

        #endregion

        #region Private



        #endregion
    }
}
