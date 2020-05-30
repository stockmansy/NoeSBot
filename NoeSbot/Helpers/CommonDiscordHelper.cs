using Discord;
using Discord.WebSocket;
using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class CommonDiscordHelper
    {
        public static Color GetColor(this SocketGuildUser user)
        {
            return user.Roles.Last().Color;
        }

        public static Embed GetHelp(string command, char[] prefixes, Color color)
        {
            var model = Labels.GetCommandInfo(command);

            var builder = new EmbedBuilder()
            {
                Color = color,
                Description = model.Description
            };

            foreach (var field in model.Fields)
            {
                builder.AddField(x =>
                {
                    x.Name = field.Name;
                    x.Value = field.Value;
                    x.IsInline = false;
                });
            }

            var i = 1;
            foreach (var example in model.Examples)
            {
                builder.AddField(x =>
                {
                    x.Name = $"{Labels.GetText("label_example")} {i}";
                    x.Value = string.Format(example, prefixes.First(), command);
                    x.IsInline = false;
                });

                i++;
            }

            if (model.Alias != null && model.Alias.Count > 0)
            {
                builder.Footer = new EmbedFooterBuilder()
                {
                    Text = $"Aliassess: {string.Join(", ", model.Alias)}"
                };
            }

            return builder.Build();
        }

        public static Embed GetModules(char[] prefixes, Color color, int[] loadedModules)
        {
            var modules = Labels.GetModules();

            var builder = new EmbedBuilder()
            {
                Color = color,
                Description = "You can use the following commands:"
            };

            foreach (var module in modules)
            {
                if (!loadedModules.Contains((int)module.Id))
                    continue;

                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var alias = cmd.Alias.Count > 0 ? $"(Alias: {string.Join(", ", cmd.Alias)})" : "";
                    description += $"{prefixes.First()}{cmd.Command} {alias}{Environment.NewLine}";
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

            return builder.Build();
        }

        public static IEmote GetEmote(string name)
        {
            return new Emoji(name);
        }
    }
}
