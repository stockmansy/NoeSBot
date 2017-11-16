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
        private IMemoryCache _cache;

        public CustomizeModule(DiscordSocketClient client, IPunishedService punishedService, IMemoryCache memoryCache)
        {
            _client = client;
            _punishedService = punishedService;
            _cache = memoryCache;
        }

        #region Commands

        #region Add Custom Punish

        [Command(Labels.Customize_AddCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task AddCustomPunish()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_AddCustomPunish_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_AddCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task AddCustomPunish([Summary("The user")] SocketGuildUser user,
                                        [Remainder] string input)
        {
            await AddCustom("punish", user, input);
        }

        #endregion

        #region Add Custom

        [Command(Labels.Customize_AddCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task AddCustom()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_AddCustom_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_AddCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task AddCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user,
                                    [Remainder] string input)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
        }

        #endregion

        #region Get Custom Punish

        [Command(Labels.Customize_GetCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task GetCustomPunish()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_GetCustomPunish_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_GetCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task GetCustomPunish([Summary("The user")] SocketGuildUser user)
        {
            await GetCustom("punish", user);
        }

        #endregion

        #region Get Custom

        [Command(Labels.Customize_GetCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task GetCustom()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_GetCustom_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_GetCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task GetCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
        }

        #endregion

        #region Remove All Custom Punish

        [Command(Labels.Customize_RemoveAllCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveAllCustomPunish()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_RemoveAllCustomPunish_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_RemoveAllCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustomPunish([Summary("The user")] SocketGuildUser user)
        {
            await RemoveCustom("punish", user);
        }

        #endregion

        #region Remove All Custom

        [Command(Labels.Customize_RemoveAllCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustom()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_RemoveAllCustom_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_RemoveAllCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook && !(Context.User.Id == user.Id))
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
        public async Task RemoveCustomPunish()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_RemoveCustomPunish_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_RemoveCustomPunish_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustomPunish([Summary("The user")] SocketGuildUser user,
                                        [Summary("The index")] int index)
        {
            await RemoveCustom("punish", user, index);
        }

        #endregion

        #region Remove Custom

        [Command(Labels.Customize_RemoveCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveSpecificCustomPunish()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Customize_RemoveCustom_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Customize_RemoveCustom_Command)]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustom([Summary("Name of the module")] string moduleName,
                                    [Summary("The user")] SocketGuildUser user,
                                    [Summary("The index")] int index)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook && !(Context.User.Id == user.Id))
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

        #endregion
    }
}
