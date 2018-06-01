using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Enums;
using NoeSbot.Extensions;
using NoeSbot.Helpers;
using NoeSbot.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class ModLogic
    {
        private readonly DiscordSocketClient _client;
        private static ConcurrentDictionary<ulong, ulong> _nukedChannels;

        public ModLogic(DiscordSocketClient client)
        {
            _client = client;
            _nukedChannels = new ConcurrentDictionary<ulong, ulong>();
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

        public async Task<bool> Process(ICommandContext context)
        {
            var user = context.User as SocketGuildUser; ;
            if (!user.GuildPermissions.Administrator && _nukedChannels.ContainsKey(context.Channel.Id))
            {
                var msg = context.Message as IMessage;
                await msg.DeleteAsync();
                return false;
            }

            return true;
        }

        public void NukeChannel(ulong channel, ulong msg)
        {
            _nukedChannels.AddOrUpdate(channel, msg);
        }

        public ulong DeNukeChannel(ulong channel)
        {
            _nukedChannels.TryRemove(channel, out ulong msg);
            return msg;
        }

        private bool ModLoaded(ulong guildId)
        {
            var loadedModules = Configuration.Load(guildId).LoadedModules;

            return loadedModules.Contains((int)ModuleEnum.Mod);
        }
    }
}
