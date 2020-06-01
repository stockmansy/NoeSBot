using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using NoeSbot.Database.Services;
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
        private readonly ISerializedItemService _serializedItemService;

        public ModLogic(DiscordSocketClient client, ISerializedItemService serializedItemService)
        {
            _client = client;
            _nukedChannels = new ConcurrentDictionary<ulong, ulong>();
            _serializedItemService = serializedItemService;
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

                await user.Guild.SystemChannel.SendMessageAsync("", false, builder.Build());

                // Add role
                var newUserRole = GlobalConfig.GetGuildConfig(guildId).NewUserRole;
                var role = user.Guild.Roles.Where(x => x.Name.Equals(newUserRole, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (role == null)
                    return;

                if (!user.Roles.Contains(role))
                    await user.AddRoleAsync(role);
            }
        }

        public async Task TakeGuildBackup(IGuild guild)
        {
            try
            {
                IReadOnlyCollection<IGuildChannel> channels = null;
                IReadOnlyCollection<IGuildUser> users = null;
                IReadOnlyCollection<IBan> bans = null;
                IGuildUser botUser = null;
                IReadOnlyCollection<IInviteMetadata> invites = null;
                IReadOnlyCollection<IRole> roles = null;
                IReadOnlyCollection<GuildEmote> emotes = null;
                try
                {
                    channels = await guild.GetChannelsAsync();
                }
                catch { }
                try
                {
                    users = await guild.GetUsersAsync();
                }
                catch { }
                try
                {
                    bans = await guild.GetBansAsync();
                } catch { }
                
                try
                {
                    await guild.GetCurrentUserAsync();
                }
                catch { }
                
                try
                {
                    invites = await guild.GetInvitesAsync();
                }
                catch { }                
                try
                {
                    roles = guild.Roles;
                }
                catch { }
                try
                {
                    emotes = guild.Emotes;
                }
                catch { }

                var channelInfo = channels?.Where(x => x.Name != null).Select(x => new
                {
                    x.Name,
                    x.Position,
                    x.Id,
                    x.CreatedAt,
                    x.PermissionOverwrites
                });

                var userInfo = users?.Select(x => new
                {
                    x.Username,
                    x.Nickname,
                    x.AvatarId,
                    x.IsBot,
                    x.IsDeafened,
                    x.IsMuted,
                    x.IsSuppressed,
                    x.JoinedAt,
                    x.RoleIds
                });
                

                var bansInfo = botUser != null && botUser.GuildPermissions.BanMembers ? bans.Select(x => new
                {
                    x.User.Id,
                    x.User.Username,
                    x.Reason
                }) : null;

                var botUserInfo = new
                {
                    botUser?.Username,
                    botUser?.Nickname,
                    botUser?.AvatarId,
                    botUser?.IsBot,
                    botUser?.IsDeafened,
                    botUser?.IsMuted,
                    botUser?.IsSuppressed,
                    botUser?.JoinedAt,
                    botUser?.RoleIds
                };

                var invitesInfo = invites?.Select(x => new
                {
                    x.Inviter.Id,
                    x.Inviter.Username,
                    x.IsRevoked,
                    x.IsTemporary,
                    x.MaxAge,
                    x.MaxUses,
                    x.Url
                });

                var rolesInfo = roles?.Select(x => new
                {
                    x.Name,
                    x.Permissions,
                    x.Position,
                    x.Color
                });

                var emotesInfo = emotes?.Select(x => new
                {
                    x.Name,
                    x.Url,
                    x.CreatedAt
                });

                var backup = new
                {
                    ChannelInfo = channelInfo,
                    UserInfo = userInfo,
                    BanInfo = bansInfo,
                    BotUserInfo = botUserInfo,
                    InviteInfo = invitesInfo,
                    RoleInfo = rolesInfo,
                    EmoteInfo = emotesInfo
                };

                await _serializedItemService.AddGuildBackup((long)guild.Id, backup);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error in Backup Guild: {ex.Message}");
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
            var loadedModules = GlobalConfig.GetGuildConfig(guildId).LoadedModules;

            return loadedModules.Contains((int)ModuleEnum.Mod);
        }
    }
}
