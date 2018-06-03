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
using NoeSbot.Extensions;
using NoeSbot.Resources;
using NoeSbot.Logic;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Mod)]
    public class ModModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly ModLogic _modLogic;
        private IMemoryCache _cache;

        #region Constructor

        public ModModule(DiscordSocketClient client, ModLogic modLogic, IMemoryCache memoryCache)
        {
            _client = client;
            _modLogic = modLogic;
            _cache = memoryCache;
        }

        #endregion

        #region Commands

        #region Nuke

        [Command(Labels.Mod_Nuke_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Nuke()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Mod_Nuke_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
        }

        [Command(Labels.Mod_Nuke_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Nuke(string time)
        {
            var timeInS = CommonHelper.GetTimeInSeconds(time);
            if (timeInS <= 0 || timeInS > 3600)
                timeInS = 60;
            var timeSpan = TimeSpan.FromSeconds(timeInS);

            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor()
            };

            builder.AddField(x =>
            {
                x.Name = "What is happending?";
                x.Value = $"{user.Username} put this channel in nuke mode for {CommonHelper.ToReadableString(timeSpan)}.{Environment.NewLine}You will not be able to send any messages unless you are a server admin.";
                x.IsInline = false;
            });

            if (user.AvatarId != null)
                builder.WithThumbnailUrl(user.GetAvatarUrl());

            var msg = await ReplyAsync("", false, builder.Build());

            var channelId = Context.Channel.Id;
            _modLogic.NukeChannel(channelId, msg.Id);

            Action endNuke = async () => await EndNuke(msg.Id, channelId, user.Username);
            endNuke.DelayFor(timeSpan);
        }

        #endregion

        #region DeNuke

        [Command(Labels.Mod_Denuke_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Denuke()
        {
            var user = Context.User as SocketGuildUser;
            var channelId = Context.Channel.Id;
            await StopNuke(channelId);
        }

        #endregion

        #region Botstatus

        [Command(Labels.Mod_Botstatus_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Botstatus()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Mod_Botstatus_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
        }

        [Command(Labels.Mod_Botstatus_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Botstatus([Remainder]string status)
        {
            await _client.SetGameAsync(status);
            await ReplyAsync("Status changed.");
        }

        #endregion

        #region Remove Messages

        [Command(Labels.Mod_RemoveMessages_Command)]
        [Alias(Labels.Mod_RemoveMessage_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveMessages()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Mod_RemoveMessages_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
        }

        [Command(Labels.Mod_RemoveMessages_Command)]
        [Alias(Labels.Mod_RemoveMessage_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveMessages(string time)
        {
            var durationInSecs = CommonHelper.GetTimeInSeconds(time);
            var deletedCount = await DeleteMessages(null, durationInSecs);
            if (deletedCount < 0)
                await ReplyAsync("Remove messages failed");
            else if (deletedCount == 0)
                await ReplyAsync("Didn't find any messages in this timeframe to remove");
            else
                await ReplyAsync($"Successfully removed {deletedCount} messages");
        }

        [Command(Labels.Mod_RemoveMessages_Command)]
        [Alias(Labels.Mod_RemoveMessage_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveMessages(SocketGuildUser user, string time)
        {
            var durationInSecs = CommonHelper.GetTimeInSeconds(time);
            var deletedCount = await DeleteMessages(user, durationInSecs);
            if (deletedCount < 0)
                await ReplyAsync("Remove messages failed");
            else if (deletedCount == 0)
                await ReplyAsync("Didn't find any messages in this timeframe to remove");
            else
                await ReplyAsync($"Successfully removed {deletedCount} messages for user {user.Username}");
        }

        #endregion

        #region Clean Messages

        [Command(Labels.Mod_CleanMessages_Command)]
        [Alias(Labels.Mod_CleanMessage_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task CleanMessages()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Mod_CleanMessages_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
        }

        [Command(Labels.Mod_CleanMessages_Command)]
        [Alias(Labels.Mod_CleanMessage_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task CleanMessages(string time)
        {
            var durationInSecs = CommonHelper.GetTimeInSeconds(time);
            await Context.Message.DeleteAsync();
            var deletedCount = await DeleteMessages(null, durationInSecs);
            if (deletedCount < 0)
                await ReplyAsync("Cleanup failed");
        }

        [Command(Labels.Mod_CleanMessages_Command)]
        [Alias(Labels.Mod_CleanMessage_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task CleanMessages(SocketGuildUser user, string time)
        {
            var durationInSecs = CommonHelper.GetTimeInSeconds(time);
            await Context.Message.DeleteAsync();
            var deletedCount = await DeleteMessages(user, durationInSecs);
            if (deletedCount < 0)
                await ReplyAsync("Cleanup failed");
        }

        #endregion

        #endregion

        #region Private

        private async Task<int> DeleteMessages(SocketGuildUser user, int secondsBack)
        {
            try
            {
                var lastMessages = await Context.Channel.GetMessagesAsync(100).Flatten();
                var userMessages = lastMessages.Where(x => (user != null ? x.Author.Id == user.Id : true) && x.CreatedAt.UtcTicks >= DateTime.UtcNow.AddSeconds(-secondsBack).Ticks).ToList();
                await Context.Channel.DeleteMessagesAsync(userMessages);

                return userMessages.Count;
            }
            catch
            {
                return -1;
            }
        }

        private async Task EndNuke(ulong messageId, ulong channelId, string username)
        {
            var msg = await Context.Channel.GetMessageAsync(messageId);
            await msg.DeleteAsync();

            await Context.Channel.SendMessageAsync($"The nuke started by {username} has ended...");

            _modLogic.DeNukeChannel(channelId);
        }

        private async Task StopNuke(ulong channelId)
        {
            var msgId = _modLogic.DeNukeChannel(channelId);
            if (msgId > 0)
            {
                var msg = await Context.Channel.GetMessageAsync(msgId);
                await msg.DeleteAsync();
            }

            await Context.Channel.SendMessageAsync($"The nuke was ended early...");
        }

        #endregion
    }
}
