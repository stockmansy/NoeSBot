using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Help)]
    public class HelpModule : ModuleBase
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("marco")]
        public async Task MarcoPoloAsync()
        {
            await ReplyAsync("Polo");
        }

        [Command("help")]
        [Summary("Retrieve a list of all commands")]
        public async Task HelpAsync()
        {
            var user = Context.User as SocketGuildUser;

            string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You can use the following commands:"
            };

            foreach (var module in _service.Modules)
            {
                if (module.Name.Equals("ignore", StringComparison.OrdinalIgnoreCase))
                    continue;

                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess) { 
                        description += $"{prefix}{cmd.Aliases.GetAliases()}";
                        description += $" -> {cmd.Summary}\n";
                    }
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        [Summary("Get info about a specific command")]
        public async Task HelpAsync(string command)
        {
            var user = Context.User as SocketGuildUser;
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, the {command} command could not be found.");
                return;
            }

            string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"Commands like {command}:"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                var value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n";
                if (!string.IsNullOrWhiteSpace(cmd.Summary))
                    value += $"Summary: {cmd.Summary}\n";
                if (!string.IsNullOrWhiteSpace(cmd.Remarks))
                    value += $"Remarks: {cmd.Remarks}";

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = value;
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
