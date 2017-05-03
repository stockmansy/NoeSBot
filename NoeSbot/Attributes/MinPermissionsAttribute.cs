using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private AccessLevel Level;

        public MinPermissionsAttribute(AccessLevel level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var access = GetPermission(context);

            if (access >= Level)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)
                return AccessLevel.Blocked;

            if (Configuration.Load(c.Guild.Id).Owners.Contains(c.User.Id))
                return AccessLevel.BotOwner;

            var user = c.User as SocketGuildUser;
            if (user != null)
            {
                if (c.Guild.OwnerId == user.Id)
                    return AccessLevel.ServerOwner;

                if (user.GuildPermissions.Administrator)
                    return AccessLevel.ServerAdmin;

                if (user.GuildPermissions.ManageMessages ||
                    user.GuildPermissions.BanMembers ||
                    user.GuildPermissions.KickMembers)
                    return AccessLevel.ServerMod;
            }

            return AccessLevel.User;
        }
    }
}
