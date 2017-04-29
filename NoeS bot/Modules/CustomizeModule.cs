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

        #region Help text

        [Command("addcustompunish")]
        [Summary("Add a custom punish rule")]
        public async Task AddCustomPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("2 required parameters: user and the punish reason");
            builder.AppendLine("1 optional parameter: delay message (the punish reason must be surrounded by \"'s");
            builder.AppendLine("This command will add a custom punish message for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("addcustom")]
        [Summary("Add a custom rule")]
        public async Task AddCustom()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("3 required parameters: module name, user and the punish reason");
            builder.AppendLine("1 optional parameter: Module specific (eg. punish => delaymessage");
            builder.AppendLine("This command will add a custom rule for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("getcustompunish")]
        [Summary("Get a custom punish rule")]
        public async Task GetCustomPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: user");
            builder.AppendLine("This command will get the custom rules for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("getcustom")]
        [Summary("Get a custom punish rule")]
        public async Task GetCustom()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("2 required parameters: module name, user");
            builder.AppendLine("This command will get the custom rules for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("removeallcustompunish")]
        [Summary("Removes all custom punish rules for a user")]
        public async Task RemoveAllCustomPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: user");
            builder.AppendLine("This command will remove all the custom rules for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("removeallcustom")]
        [Summary("Removes all custom rules for a user")]
        public async Task RemoveCustom()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("2 required parameters: module name, user");
            builder.AppendLine("This command will remove all the custom rules for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("removecustompunish")]
        [Summary("Removes a specific custom rules for a user")]
        public async Task RemoveCustomPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("2 required parameters: user, index");
            builder.AppendLine("This command will remove a specific custom rule for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("removecustompunish")]
        [Summary("Removes a specific custom rules for a user")]
        public async Task RemoveSpecificCustomPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("3 required parameters: module name, user and index");
            builder.AppendLine("This command will remove a specific custom rule for a user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        #endregion

        #region Commands

        [Command("addcustompunish")]
        [Summary("Add a custom punish rule")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task AddCustomPunish([Summary("The user")] SocketGuildUser user,
                                        [Remainder] string input)
        {
            await AddCustom("punish", user, input);
        }

        [Command("addcustom")]
        [Summary("Add a custom rule")]
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
                        if (success) {
                            var result = $"Added a custom punish message for {user.Mention}{Environment.NewLine}";
                            result += $"Custom message:{Environment.NewLine}{reason.GetProcessedString()}{Environment.NewLine}{Environment.NewLine}";
                            await ReplyAsync(result);

                            if (hasDelayMsg) {
                                result = $"Custom delay message:{Environment.NewLine}{delaymessage.GetProcessedString()}";
                                await ReplyAsync(result);
                            }
                        } else
                        {
                            await ReplyAsync("Failed to save the custom punish rule");
                        }
                        break;
                }
            }
        }

        [Command("getcustompunish")]
        [Summary("Get a custom punish rule")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task GetCustomPunish([Summary("The user")] SocketGuildUser user)
        {
            await GetCustom("punish", user);
        }

        [Command("getcustom")]
        [Summary("Get all custom rules")]
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
                        if (existing == null || existing.Count() == 0) {
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

                            if (!string.IsNullOrWhiteSpace(custom.DelayMessage)) {
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

        [Command("removeallcustompunish")]
        [Summary("Removes all custom punish rules for a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustomPunish([Summary("The user")] SocketGuildUser user)
        {
            await RemoveCustom("punish", user);
        }

        [Command("removeallcustom")]
        [Summary("Removes all custom rules for a user")]
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
            } else
            {
                await ReplyAsync("https://cdn.discordapp.com/attachments/285808114052104193/289916801502937089/img.png");
            }
        }

        [Command("removecustompunish")]
        [Summary("Remove a custom punish rule")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task RemoveCustomPunish([Summary("The user")] SocketGuildUser user,
                                        [Summary("The index")] int index)
        {
            await RemoveCustom("punish", user, index);
        }

        [Command("removecustom")]
        [Summary("Remove a custom rule")]
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
    }
}
