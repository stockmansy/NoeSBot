using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using NoeSbot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using NoeSbot.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Database;
using System.Threading;
using System.Net;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Utility)]
    public class UtilityModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private static IEnumerable<IMessage> _recentMediaMessages = null;

        #region Constructor

        public UtilityModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        #endregion

        #region Help text


        [Command("userinfo")]
        [Summary("Get info regarding a certain user")]
        [MinPermissions(AccessLevel.User)]
        public async Task UserInfo()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "You can retrieve info about a user with this command."
                };

                builder.AddField(x =>
                {
                    x.Name = "Example";
                    x.Value = "~userinfo @MensAap";
                    x.IsInline = false;
                });

                await ReplyAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Commands

        [Command("userinfo")]
        [Summary("Get info regarding a certain user")]
        [MinPermissions(AccessLevel.User)]
        public async Task UserInfo(SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor()
                };

                builder.AddField(x =>
                {
                    x.Name = $"Username of {user.Nickname}";
                    x.Value = $"{user.Username}";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "Join date";
                    x.Value = $"The user's joined {user.JoinedAt?.ToString("dd/MM/yyyy HH:mm")}";
                    x.IsInline = false;
                });

                builder.AddField(x =>
                {
                    x.Name = "The user roles";
                    x.Value = $"{string.Join(Environment.NewLine, user.Roles.Where(y => y.Id != y.Guild.EveryoneRole.Id).Select(y => y.Name))}";
                    x.IsInline = false;
                });

                if (user.AvatarId != null)
                    builder.WithThumbnailUrl(user.GetAvatarUrl());

                await ReplyAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Private



        #endregion
    }
}
