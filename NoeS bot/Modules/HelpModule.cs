using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Modules
{
    public class HelpModule : ModuleBase
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var user = Context.User as SocketGuildUser;

            string prefix = Configuration.Load().Prefix.ToString();
            var builder = new EmbedBuilder()
            {

                Color = user.Roles.First().Color,
                Description = "You can use the following commands:"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{prefix}{cmd.Summary}\n";
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
        public async Task HelpAsync(string command)
        {
            var user = Context.User as SocketGuildUser;
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, the {command} command could not be found.");
                return;
            }

            string prefix = Configuration.Load().Prefix.ToString();
            var builder = new EmbedBuilder()
            {
                Color = user.Roles.First().Color,
                Description = $"Commands like {command}:"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Remarks: {cmd.Remarks}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
