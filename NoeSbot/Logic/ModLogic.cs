using Discord;
using Discord.WebSocket;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class ModLogic
    {
        private readonly DiscordSocketClient _client;

        public ModLogic(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            var guildId = user.Guild.Id;
            if (ModLoaded(guildId))
            {
                //  Send welcome message
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor()
                };

                builder.AddField(x =>
                {
                    x.Name = "Welcome!";
                    x.Value = $"Welcome to the server {user.Username}!";
                    x.IsInline = false;
                });


                if (user.AvatarId != null)
                    builder.WithThumbnailUrl(user.GetAvatarUrl());

                await user.Guild.DefaultChannel.SendMessageAsync("", false, builder.Build());

                // Add role
                var newUserRole = Configuration.Load(guildId).NewUserRole;
                var role = user.Guild.Roles.Where(x => x.Name.Equals(newUserRole, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (role == null)
                    return;

                if (!user.Roles.Contains(role))
                    await user.AddRoleAsync(role);
            }
        }

        private bool ModLoaded (ulong guildId)
        {
            var loadedModules = Configuration.Load(guildId).LoadedModules;

            return loadedModules.Contains((int)ModuleEnum.Mod);
        }
    }
}
