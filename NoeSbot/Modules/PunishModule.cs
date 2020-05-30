using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
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
using NoeSbot.Database.Services;
using NoeSbot.Database.Models;
using NoeSbot.Logic;
using NoeSbot.Resources;
using Microsoft.Extensions.Logging;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Punish)]
    public class PunishModule : ModuleBase
    {
        private const int PUNISH_TIME = 3000;

        private PunishLogic _logic;
        private readonly ILogger<PunishModule> _logger;

        #region Constructor

        public PunishModule(PunishLogic logic, ILoggerFactory loggerFactory)
        {
            _logic = logic;
            _logger = loggerFactory.CreateLogger<PunishModule>();
        }

        #endregion

        #region Commands

        #region Punish

        [Command(Labels.Punish_Punish_Command)]
        [Alias(Labels.Punish_Punish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Punish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Punish_Punish_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Punish_Punish_Command)]
        [Alias(Labels.Punish_Punish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user)
        {
            await Punish(user, "5m", "No reason given");
        }

        [Command(Labels.Punish_Punish_Command)]
        [Alias(Labels.Punish_Punish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user,
                                 [Summary("The punish time")]string time)
        {
            await Punish(user, time, "No reason given");
        }

        [Command(Labels.Punish_Punish_Command)]
        [Alias(Labels.Punish_Punish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user,
                                 [Summary("The punish time")]string time,
                                 [Remainder, Summary("The punish reason")]string reason)
        {
            if (!user.IsBot)
            {
                try
                {
                    var durationInSecs = CommonHelper.GetTimeInSeconds(time);
                    var punishTime = CommonHelper.ToReadableString(TimeSpan.FromSeconds(durationInSecs));

                    var cus = await _logic.GetCustomPunish(user);
                    if (!cus.HasCustom)
                    {
                        var randomPunishedImage = await _logic.Punish(Context, user, time, reason, durationInSecs);
                        if (randomPunishedImage == null)
                            await Context.Channel.SendMessageAsync($"Successfully punished {user.Mention} ({user.Username}) for {punishTime}");
                        else
                            await Context.Channel.SendFileAsync(randomPunishedImage, $"Successfully punished {user.Mention} ({user.Username}) for {punishTime}");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(cus.DelayMessage))
                        {
                            await ReplyAsync(cus.DelayMessage);
                            await Task.Delay(PUNISH_TIME);
                        }

                        await _logic.Punish(Context, user, time, reason, durationInSecs);
                        await ReplyAsync(cus.ReasonMessage);
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"Failed to punish {user.Username}";
                    _logger.LogError($"Error in Punish: {ex.Message}");
                    LogHelper.LogDebug(msg, false);
                    await ReplyAsync(msg);
                }
            }
        }

        #endregion

        #region Punished

        [Command(Labels.Punish_Punished_Command)]
        [Alias(Labels.Punish_Punished_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Punished()
        {
            var allPunished = await _logic.GetPunished(Context);

            var user = Context.User as SocketGuildUser;
            var count = allPunished.Count();
            var end = (count <= 0) ? $"{Environment.NewLine}None" : "";
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"The following users were punished:{end}"
            };

            foreach (var pun in allPunished)
            {
                var punUser = await Context.Client.GetUserAsync((ulong)pun.UserId);
                var punishTime = CommonHelper.GetTimeString(pun.TimeOfPunishment, pun.Duration);

                builder.AddField(x =>
                {
                    x.Name = punUser.Username;
                    x.Value = $"Punished for: {punishTime}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Unpunish

        [Command(Labels.Punish_Unpunish_Command)]
        [Alias(Labels.Punish_Unpunish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UnPunish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Punish_Unpunish_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Punish_Unpunish_Command)]
        [Alias(Labels.Punish_Unpunish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UnPunish([Summary("The user to be unpunished")] SocketGuildUser user)
        {
            var msg = "";
            var success = await _logic.UnPunish(Context, user);
            if (success.HasValue)
            {
                msg = (success.Value) ? $"Successfully unpunished {user.Mention} ({user.Username})" : $"Failed to unpunish {user.Username}";
                await ReplyAsync(msg);
            }
        }

        [Command(Labels.Punish_Unpunish_Command)]
        [Alias(Labels.Punish_Unpunish_Alias_1)]
        [MinPermissions(AccessLevel.ServerMod)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UnPunish([Remainder, Summary("The punish input")]string input)
        {
            if (input.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var msg = "";
                var success = await _logic.UnPunishAll(Context);
                if (success.HasValue)
                {
                    msg = (success.Value) ? $"Successfully unpunished everybody" : $"Failed to unpunish everybody";
                    await ReplyAsync(msg);
                }
            }
        }

        #endregion

        #endregion

        #region Private

        #endregion
    }
}
