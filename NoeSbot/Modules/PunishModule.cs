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
using NoeSbot.Database.Models;
using NoeSbot.Logic;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Punish)]
    public class PunishModule : ModuleBase
    {
        private PunishLogic _logic;

        #region Constructor

        public PunishModule(PunishLogic logic)
        {
            _logic = logic;
        }

        #endregion

        #region Help text

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user) (Defaults to 5m No reason given)")]
        public async Task Punish()
        {
            var user = Context.User as SocketGuildUser;
            string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "Punish a user (Defaults to 5m No reason given)",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Permission: Mod minimum"
                }
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter 1 (Required)";
                x.Value = $"User (The user you want to punish)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Parameter 2 (Optional)";
                x.Value = "Time (e.g. 10m)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Parameter 3 (Optional)";
                x.Value = "Reason (e.g. Was being a dick)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example";
                x.Value = $"{prefix}punish @MensAap";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example 2";
                x.Value = $"{prefix}punish @MensAap 10m";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example 3 ";
                x.Value = $"{prefix}punish @MensAap For some weird reason...";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example 4";
                x.Value = $"{prefix}punish @MensAap 10m For some weird reason...";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        [Command("unpunish")]
        [Alias("unsilence")]
        [Summary("Unpunish specific user")]
        public async Task UnPunish()
        {
            var user = Context.User as SocketGuildUser;
            string prefix = Configuration.Load(Context.Guild.Id).Prefix.ToString();
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "Unpunish a user",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Permission: Mod minimum"
                }
            };

            builder.AddField(x =>
            {
                x.Name = "Parameter 1 (Required)";
                x.Value = $"User (The user you want to unpunish) OR all (If you want to unpunish all)";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example";
                x.Value = $"{prefix}unpunish @MensAap";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Example";
                x.Value = $"{prefix}unpunish all";
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Commands

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user) (Defaults to 5m No reason given)")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user)
        {
            await Punish(user, "5m", "No reason given");
        }

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user, time) (Defaults to No reason given)")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user,
                                 [Summary("The punish time")]string time)
        {
            await Punish(user, time, "No reason given");
        }

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user time reason)")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user,
                                 [Summary("The punish time")]string time,
                                 [Remainder, Summary("The punish reason")]string reason)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook && !user.IsBot)
            {
                try
                {
                    var res = await _logic.Punish(Context, user, time, reason);
                    if (!res.HasCustom)
                        await Context.Channel.SendFileAsync(Globals.RandomPunishedImage.FullName, $"Successfully punished {user.Mention} ({user.Username}) for {res.PunishTime}");
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(res.DelayMessage))
                        {
                            await ReplyAsync(res.DelayMessage);
                            await Task.Delay(3000);
                        }

                        await ReplyAsync(res.ReasonMessage);
                    }
                }
                catch
                {
                    // TODO logging
                    await ReplyAsync($"Failed to punish {user.Username}");
                }
            }
        }

        [Command("punished")]
        [Alias("silenced")]
        [Summary("List of the punished users")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punished()
        {
            var allPunished = await _logic.GetPunished(Context);

            var user = Context.User as SocketGuildUser;
            var count = allPunished.Count();
            var end = (count <= 0) ? $"{Environment.NewLine}None" : "";
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"The following users were punished:{end}"
            };
            
            foreach (var pun in allPunished)
            {
                var punUser = await Context.Client.GetUserAsync((ulong)pun.UserId);
                var punishTime = CommonHelper.GetTimeString(pun.TimeOfPunishment, pun.Duration);

                builder.AddField(x =>
                {
                    x.Name = punUser.Username;
                    x.Value = $"Punished for: {punishTime}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("unpunish")]
        [Alias("unsilence")]
        [Summary("Unpunish specific user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task UnPunish([Summary("The user to be unpunished")] SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var msg = "";
                var success = await _logic.UnPunish(Context, user);
                if (success.HasValue)
                {
                    msg = (success.Value) ? $"Successfully unpunished {user.Mention} ({user.Username})" : $"Failed to unpunish {user.Username}";
                    await ReplyAsync(msg);
                }
            }
        }

        [Command("unpunish")]
        [Alias("unsilence")]
        [Summary("Unpunish all users")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task UnPunish([Remainder, Summary("The punish input")]string input)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (input.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = "";
                    var success = await _logic.UnPunishAll(Context);
                    if (success.HasValue)
                    {
                        msg = (success.Value) ? $"Successfully unpunished everybody" : $"Failed to unpunish everybody";
                        await ReplyAsync(msg);
                    }
                }
            }
        }

        #endregion

        #region Private
                
        #endregion
    }
}
