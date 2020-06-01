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
using NoeSbot.Resources;

namespace NoeSbot.Modules
{
    [Name("Ignore")]
    [ModuleName(ModuleEnum.Customize)]
    public class CustomizeModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly IPunishedService _punishedService;
        private readonly IProfileService _profileService;
        private readonly ICustomCommandService _customCommandService;
        private IMemoryCache _cache;

        public CustomizeModule(DiscordSocketClient client, IPunishedService punishedService, IProfileService profileService, ICustomCommandService customCommandService, IMemoryCache memoryCache)
        {
            _client = client;
            _punishedService = punishedService;
            _profileService = profileService;
            _cache = memoryCache;
            _customCommandService = customCommandService;
        }

        #region Commands

        #region Add Custom Punish

        [Command(Labels.Customize_AddCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomPunish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_AddCustomPunish_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_AddCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomPunish([Summary("The user")] SocketGuildUser user,
                                        [Remainder] string input)
        {
            await AddCustom("punish", user, input);
        }

        #endregion

        #region Add Custom

        [Command(Labels.Customize_AddCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustom()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_AddCustom_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_AddCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user,
                                    [Remainder] string input)
        {
            switch (moduleName)
            {
                case "punish":
                    var reason = input;
                    var delaymessage = "";
                    var lastIndex = input.LastIndexOf('"');
                    if (input.StartsWith("\"") && lastIndex > 0)
                    {
                        reason = input.Substring(1, lastIndex - 1);
                        delaymessage = input.Substring(lastIndex + 1);
                    }

                    var hasDelayMsg = !string.IsNullOrWhiteSpace(delaymessage);

                    var success = await _punishedService.SaveCustomPunishedAsync((long)user.Id, reason, delaymessage);
                    if (success)
                    {
                        var result = $"Added a custom punish message for {user.Mention}{Environment.NewLine}";
                        result += $"Custom message:{Environment.NewLine}{reason.GetProcessedString()}{Environment.NewLine}{Environment.NewLine}";
                        await ReplyAsync(result);

                        if (hasDelayMsg)
                        {
                            result = $"Custom delay message:{Environment.NewLine}{delaymessage.GetProcessedString()}";
                            await ReplyAsync(result);
                        }
                    }
                    else
                    {
                        await ReplyAsync("Failed to save the custom punish rule");
                    }
                    break;
            }
        }

        #endregion

        #region Add Custom Profile Background

        [Command(Labels.Customize_AddCustomProfileBackground_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomProfileBackground()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_AddCustomProfileBackground_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_AddCustomProfileBackground_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomProfileBackground(SocketGuildUser user,
                                                     string url)
        {
            var success = await _profileService.AddOrUpdateProfileBackground((long)Context.Guild.Id, Database.Models.ProfileBackground.ProfileBackgroundSetting.Custom, url, (long)user.Id);
            if (success)
                await ReplyAsync($"Successfully added a custom background for {user.Nickname}");
            else
                await ReplyAsync($"Failed to add a custom background for {user.Nickname}");
        }

        [Command(Labels.Customize_AddCustomProfileBackground_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomProfileBackground(string customtype,
                                                    string url,
                                                    [Remainder] string aliases)
        {
            if (!customtype.Equals("game", StringComparison.OrdinalIgnoreCase))
            {
                await ReplyAsync($"Invalid custom type");
                return;
            }

            var aliasesList = aliases.Trim().Split(' ').ToList();

            var success = await _profileService.AddOrUpdateProfileBackground((long)Context.Guild.Id, Database.Models.ProfileBackground.ProfileBackgroundSetting.Game, url, (long)Context.User.Id, aliasesList);
            if (success)
                await ReplyAsync($"Successfully added a custom background for a game");
            else
                await ReplyAsync($"Failed to add a custom background for a game");
        }

        #endregion

        #region Add Custom Punish Command

        [Command(Labels.Customize_AddCustomPunishCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomPunishedCommand()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_AddCustomPunishCommand_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_AddCustomPunishCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomPunishCommand(string customCommand,
                                                   SocketGuildUser user)
        {
            await AddCustomPunishCommand(customCommand, user, "5m");
        }

        [Command(Labels.Customize_AddCustomPunishCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomPunishCommand(string customCommand,
                                                   SocketGuildUser user,
                                                   string time)
        {
            await AddCustomPunishCommand(customCommand, user, time, "No reason given");
        }

        [Command(Labels.Customize_AddCustomPunishCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomPunishCommand(string customCommand,
                                                   SocketGuildUser user,
                                                   string time,
                                                   [Remainder] string reason)
        {
            var durationInSecs = CommonHelper.GetTimeInSeconds(time);

            var success = await _customCommandService.SaveCustomPunishCommandAsync(customCommand, (long)Context.Guild.Id, (long)user.Id, durationInSecs, reason);
            if (success)
            {
                await ReplyAsync($"Successfully created the custom punish command");
            }

            RemoveCache();
        }

        #region Add Custom Alias Command

        [Command(Labels.Customize_AddCustomAliasCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomAliasCommand()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_AddCustomAliasCommand_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_AddCustomAliasCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomAliasCommand(string customCommand,
                                                string aliasCommand)
        {
            await AddCustomAliasCommand(customCommand, aliasCommand, false);
        }

        [Command(Labels.Customize_AddCustomAliasCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomAliasCommand(string customCommand,
                                                string aliasCommand,
                                                bool removeMessages)
        {
            var success = await _customCommandService.SaveCustomAliasCommandAsync(customCommand.RemovePrefix(GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes), (long)Context.Guild.Id, aliasCommand, removeMessages);
            if (success)
            {
                await ReplyAsync($"Successfully created the custom alias command");
            }

            RemoveCache();
        }

        #endregion

        #endregion

        #region Add Custom Unpunish Command

        [Command(Labels.Customize_AddCustomUnpunishCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomUnpunishCommand()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_AddCustomUnpunishCommand_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_AddCustomUnpunishCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddCustomUnpunishCommand(string customCommand,
                                                   SocketGuildUser user)
        {
            var success = await _customCommandService.SaveCustomUnpunishCommandAsync(customCommand, (long)Context.Guild.Id, (long)user.Id);
            if (success)
            {
                await ReplyAsync($"Successfully created the custom unpunish command");
            }

            RemoveCache();
        }

        #endregion

        #region Get Custom Punish

        [Command(Labels.Customize_GetCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetCustomPunish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_GetCustomPunish_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_GetCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetCustomPunish([Summary("The user")] SocketGuildUser user)
        {
            await GetCustom("punish", user);
        }

        #endregion

        #region Get Custom

        [Command(Labels.Customize_GetCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetCustom()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_GetCustom_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_GetCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user)
        {
            switch (moduleName)
            {
                case "punish":
                    var existing = await _punishedService.RetrieveAllCustomPunishedAsync((long)user.Id);
                    if (existing == null || existing.Count() == 0)
                    {
                        await ReplyAsync("This user does not have any custom rules");
                        return;
                    }

                    var builder = new EmbedBuilder()
                    {
                        Color = user.GetColor(),
                        Description = "This user has the following custom rules:"
                    };

                    var i = 1;
                    foreach (var custom in existing)
                    {
                        var description = custom.Reason;
                        builder.AddField(x =>
                        {
                            x.Name = $"Custom {i} reason";
                            x.Value = description.GetProcessedString();
                            x.IsInline = false;
                        });

                        if (!string.IsNullOrWhiteSpace(custom.DelayMessage))
                        {
                            description = custom.DelayMessage;
                            builder.AddField(x =>
                            {
                                x.Name = $"Custom {i} delay message";
                                x.Value = description.GetProcessedString();
                                x.IsInline = false;
                            });
                        }
                        i++;
                    }

                    await ReplyAsync("", false, builder.Build());

                    break;
            }
        }

        #endregion

        #region Remove All Custom Punish

        [Command(Labels.Customize_RemoveAllCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveAllCustomPunish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_RemoveAllCustomPunish_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_RemoveAllCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomPunish([Summary("The user")] SocketGuildUser user)
        {
            await RemoveCustom("punish", user);
        }

        #endregion

        #region Remove All Custom

        [Command(Labels.Customize_RemoveAllCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustom()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_RemoveAllCustom_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_RemoveAllCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user)
        {
            if (!(Context.User.Id == user.Id))
            {
                switch (moduleName)
                {
                    case "punish":
                        var success = await _punishedService.RemoveCustomPunishedAsync((long)user.Id);
                        if (success)
                            await ReplyAsync("Successfully removed all rules from a user");
                        else
                            await ReplyAsync("Failed to remove all rules from a user");
                        break;
                }
            }
            else
            {
                await ReplyAsync("https://cdn.discordapp.com/attachments/285808114052104193/289916801502937089/img.png");
            }
        }

        #endregion

        #region Remove Custom Punish

        [Command(Labels.Customize_RemoveCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomPunish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_RemoveCustomPunish_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_RemoveCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomPunish([Summary("The user")] SocketGuildUser user,
                                        [Summary("The index")] int index)
        {
            await RemoveCustom("punish", user, index);
        }

        #endregion

        #region Remove Custom

        [Command(Labels.Customize_RemoveCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveSpecificCustomPunish()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_RemoveCustom_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_RemoveCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user,
                                    [Summary("The index")] int index)
        {
            if (!(Context.User.Id == user.Id))
            {
                switch (moduleName)
                {
                    case "punish":
                        var success = await _punishedService.RemoveCustomPunishedAsync((long)user.Id, (index - 1));
                        if (success)
                            await ReplyAsync("Successfully removed a rule from a user");
                        else
                            await ReplyAsync("Failed to remove a rule from a user");
                        break;
                }
            }
            else
            {
                await ReplyAsync("https://cdn.discordapp.com/attachments/285808114052104193/289916801502937089/img.png");
            }
        }

        #endregion

        #region Remove Custom Command

        [Command(Labels.Customize_RemoveCustomCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomCommand()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_RemoveCustomCommand_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_RemoveCustomCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomCommand(string customCommand)
        {
            var success = await _customCommandService.RemoveCustomCommandAsync(customCommand, (long)Context.Guild.Id);
            if (success)
            {
                await ReplyAsync($"Successfully removed the custom command");
            }

            RemoveCache();
        }

        #endregion

        #region Remove Custom Alias Command

        [Command(Labels.Customize_RemoveCustomAliasCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomAliasCommand()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Customize_RemoveCustomAliasCommand_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Customize_RemoveCustomAliasCommand_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task RemoveCustomAliasCommand(string customCommand)
        {
            var success = await _customCommandService.RemoveCustomCommandAsync(customCommand.RemovePrefix(GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes), (long)Context.Guild.Id);
            if (success)
            {
                await ReplyAsync($"Successfully removed the custom alias command");
            }

            RemoveCache();
        }

        #endregion

        #endregion

        #region Private 

        public void RemoveCache()
        {
            _cache.Remove(CacheEnum.CustomCommmands);
        }

        #endregion
    }
}
