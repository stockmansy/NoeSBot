using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using System;
using System.Threading.Tasks;
using System.Linq;
using NoeSbot.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Resources;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Utility)]
    public class UtilityModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private Random _random = new Random();

        #region Constructor

        public UtilityModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        #endregion

        #region Commands

        #region User Info

        [Command(Labels.Utility_UserInfo_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UserInfo()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Utility_UserInfo_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
        }


        [Command(Labels.Utility_UserInfo_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UserInfo(SocketGuildUser user)
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

        #endregion

        #region Random Member

        [Command(Labels.Utility_RandomMember_Command)]
        [Alias(Labels.Utility_RandomMember_Alias_1, Labels.Utility_RandomMember_Alias_2)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RandomMember()
        {
            await Context.Message.DeleteAsync();
            var allUsers = await Context.Guild.GetUsersAsync();
            var onlineUsers = allUsers.Where(x => x.Status == UserStatus.Online && !x.IsBot && !x.IsWebhook).ToList();
            var rndUser = onlineUsers[_random.Next(onlineUsers.Count)];
            var name = !string.IsNullOrWhiteSpace(rndUser.Nickname) ? rndUser.Nickname : rndUser.Username;
            await ReplyAsync($"Picked {name} at random");
        }

        #endregion

        #endregion

        #region Private



        #endregion
    }
}
