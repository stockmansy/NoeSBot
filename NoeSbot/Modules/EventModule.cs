using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using System;
using System.Threading.Tasks;
using System.Linq;
using NoeSbot.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Resources;
using NoeSbot.Models;
using System.Collections.Concurrent;
using System.Threading;
using NoeSbot.Database.Services;
using NoeSbot.Logic;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Event)]
    public class EventModule : ModuleBase
    {
        private readonly Random _random = new Random();
        private readonly IEventService _eventService;
        private readonly EventLogic _eventLogic;

        #region Constructor

        public EventModule(IEventService eventService, EventLogic eventLogic)
        {
            _eventService = eventService;
            _eventLogic = eventLogic;
        }

        #endregion

        #region Commands

        #region Start Event

        [Command(Labels.Event_StartEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task StartEvent()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Event_StartEvent_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Event_StartEvent_Command)]
        [Alias(Labels.Event_StartEvent_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task StartEvent(string eventtype, string uniqueidentifier, string name, string description, string datetime)
        {
            var user = Context.User as SocketGuildUser;
            EventEmum type;

            try
            {
                if (string.IsNullOrWhiteSpace(uniqueidentifier.Trim()))
                    throw new Exception("Invalid unique identifier");

                DateTime? matchdate = null;

                var date = CommonHelper.GetDate(datetime);
                if (!date.HasValue)
                    throw new Exception("Invalid date");

                var dt = date.Value;

                if (dt <= DateTime.Now)
                    throw new Exception("Date in the past");

                switch (eventtype.ToLowerInvariant())
                {
                    case "ss":
                    case "secretsanta":
                    case "secret santa":
                        type = EventEmum.SecretSanta;
                        matchdate = dt.AddDays(-7);
                        if (matchdate <= DateTime.Now)
                        {
                            var diff = dt - DateTime.Now;
                            matchdate = dt.AddTicks(diff.Ticks / 2);
                        }
                        break;
                    default:
                        throw new Exception("Invalid type");
                }

                var descr = description.Replace("###", Environment.NewLine);

                var eventItem = new EventItem
                {
                    Name = name,
                    Description = descr,
                    UniqueIdentifier = uniqueidentifier,
                    Type = type,
                    Date = dt,
                    MatchDate = matchdate
                };

                await _eventService.AddEventItem((long)Context.Guild.Id, (long)user.Id, uniqueidentifier, name, descr, (int)type, dt.ToUniversalTime(), matchdate);

                await PrintEventNotification(eventItem);
            }
            catch (Exception ex)
            {
                LogHelper.LogWarning($"An event was improperly started: {ex}");

                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "You didn't start the event properly, make sure you have a unique id. Refer to the help text"
                };

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Update Event

        [Command(Labels.Event_UpdateEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UpdateEvent()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Event_UpdateEvent_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Event_UpdateEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UpdateEvent(string uniqueidentifier, string name, string description, string datetime)
        {
            var user = Context.User as SocketGuildUser;

            try
            {
                if (string.IsNullOrWhiteSpace(uniqueidentifier.Trim()))
                    throw new Exception("Invalid unique identifier");

                DateTime? matchdate = null;

                var date = CommonHelper.GetDate(datetime);
                if (!date.HasValue)
                    throw new Exception("Invalid date");

                var dt = date.Value;

                if (dt <= DateTime.Now)
                    throw new Exception("Date in the past");

                if (matchdate != null)
                {
                    matchdate = dt.AddDays(-7);
                    if (matchdate <= DateTime.Now)
                    {
                        var diff = dt - DateTime.Now;
                        matchdate = dt.AddTicks(diff.Ticks / 2);
                    }
                }

                var descr = description.Replace("###", Environment.NewLine);

                var eventItem = new EventItem
                {
                    Name = name,
                    Description = descr,
                    UniqueIdentifier = uniqueidentifier,
                    Date = dt,
                    MatchDate = matchdate
                };

                await _eventService.UpdateEventItem((long)Context.Guild.Id, (long)user.Id, uniqueidentifier, name, descr, dt.ToUniversalTime(), matchdate);

                await PrintEventNotification(eventItem);
            }
            catch
            {
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "You didn't update the event properly, make sure you have the correct unique id. Refer to the help text"
                };

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Stop Event

        [Command(Labels.Event_StopEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task StopEvent()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Event_StopEvent_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Event_StopEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task StopEvent(string uniqueidentifier)
        {
            var user = Context.User as SocketGuildUser;

            try
            {
                if (await _eventService.DisableEventItem((long)Context.Guild.Id, uniqueidentifier))
                {
                    await user.SendMessageAsync("Succesfully stopped the event");
                }
                else
                    throw new Exception("Failed to stop the event");
            }
            catch
            {
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "Failed to stop the event"
                };

                await user.SendMessageAsync("", false, builder.Build());
            }
        }

        #endregion

        #region Trigger Event

        [Command(Labels.Event_TriggerEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task TriggerEvent()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Event_TriggerEvent_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Event_TriggerEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task TriggerEvent(string uniqueidentifier)
        {
            var user = Context.User as SocketGuildUser;

            try
            {
                var eventItem = await _eventService.RetrieveEventAsync((long)Context.Guild.Id, uniqueidentifier);
                if (eventItem != null)
                {
                    await Context.Message.DeleteAsync();

                    switch (eventItem.Type)
                    {
                        case (int)EventEmum.SecretSanta:
                            var partsTasks = eventItem.Participants.Select(async x =>
                            {
                                return await Context.Channel.GetUserAsync((ulong)x.UserId);
                            });
                            var parts = await Task.WhenAll(partsTasks);
                            var participants = parts.ToList();

                            //TODO fix this silly holiday mindset logic
                            var pooled = true;
                            var users = new List<EventUser>();
                            do
                            {
                                users.Clear();

                                foreach (var p in CommonHelper.Shuffle(parts))
                                {
                                    var found = false;
                                    var random = -1;
                                    IUser ss = null;
                                    while (!found)
                                    {
                                        random = _random.Next(participants.Count);
                                        ss = participants.ElementAt(random);
                                        if (ss.Id != p.Id)
                                            found = true;
                                        else if (participants.Count <= 1)
                                        {
                                            pooled = false;
                                            break;
                                        }
                                    }

                                    participants.RemoveAt(random);

                                    users.Add(new EventUser
                                    {
                                        User = p,
                                        SecretSanta = ss
                                    });
                                }
                            } while (!pooled);

                            var orgsTasks = eventItem.Organisers.Select(async x =>
                            {
                                return await Context.Channel.GetUserAsync((ulong)x.UserId);
                            });
                            var orgs = await Task.WhenAll(orgsTasks);

                            var embed = GetSecretSantaAuthorEmbed(eventItem.Name, eventItem.Description, users);
                            foreach (var o in orgs)
                            {
                                await o.SendMessageAsync("", false, embed);
                            }

                            foreach (var u in users)
                            {
                                var msg = GetSecretSantaMatchEmbed(eventItem.Name, u);
                                await u.User.SendMessageAsync("", false, msg);
                            }

                            break;
                    }



                    //var embed = GetParticipantsEmbed(parts, item);
                    //await userAdjusting.SendMessageAsync("", false, embed);
                }
            }
            catch
            {
                var builder = new EmbedBuilder()
                {
                    Color = user.GetColor(),
                    Description = "Failed to trigger the event"
                };

                await user.SendMessageAsync("", false, builder.Build());
            }
        }

        #endregion

        #endregion

        #region Private

        private async Task PrintEventNotification(EventItem item)
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"{user.Nickname} has started an event",
                Footer = new EmbedFooterBuilder { Text = $"Click the icons below to register/unregister/view participants {{u:{item.UniqueIdentifier}}}" }
            };

            builder.AddField(x =>
            {
                x.Name = $"{item.Name}";
                x.Value = $"The event will end on {item.Date.ToLocalTime():dd-MM-yyyy hh:mm}";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Description";
                x.Value = item.Description;
                x.IsInline = false;
            });

            if (item.Type == EventEmum.SecretSanta && item.MatchDate.HasValue)
            {
                builder.AddField(x =>
                {
                    x.Name = $"Match date";
                    x.Value = $"Secret santa matches will be send out on {item.MatchDate.Value.ToLocalTime():dd-MM-yyyy hh:mm}";
                    x.IsInline = false;
                });
            }

            if (Context.Guild.IconUrl != null)
                builder.WithThumbnailUrl(Context.Guild.IconUrl);

            var message = await Context.Channel.SendMessageAsync("", false, builder.Build());

            await message.AddReactionAsync(CommonDiscordHelper.GetEmote(IconHelper.Bell));
            await Task.Delay(1250);
            await message.AddReactionAsync(CommonDiscordHelper.GetEmote(IconHelper.BellStop));
            await Task.Delay(1250);
            await message.AddReactionAsync(CommonDiscordHelper.GetEmote(IconHelper.ClipBoard));

            _eventLogic.AddEvent(message.Id, item);
        }

        private Embed GetSecretSantaAuthorEmbed(string eventName, string eventDescription, IEnumerable<EventUser> users)
        {
            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"{user.Nickname} has triggered the event",
                Footer = new EmbedFooterBuilder { Text = $"Please file an issue for the bot if this list is somehow wrong." }
            };

            builder.AddField(x =>
            {
                x.Name = $"{eventName}";
                x.Value = $"The event has ended";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Description";
                x.Value = eventDescription;
                x.IsInline = false;
            });

            var strBuilder = new StringBuilder();
            foreach (var u in users)
            {
                var usr = u.User as SocketGuildUser;
                var ssusr = u.SecretSanta as SocketGuildUser;
                strBuilder.AppendLine($"{usr.Nickname} ({usr.Username}) has {ssusr.Nickname} ({ssusr.Username}) as a match");
            }

            builder.AddField(x =>
            {
                x.Name = $"Matches";
                x.Value = $"{strBuilder}";
                x.IsInline = false;
            });

            if (Context.Guild.IconUrl != null)
                builder.WithThumbnailUrl(Context.Guild.IconUrl);

            return builder.Build();
        }

        private Embed GetSecretSantaMatchEmbed(string eventName, EventUser u)
        {

            var user = Context.User as SocketGuildUser;
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = $"{user.Nickname} has triggered the event",
                Footer = new EmbedFooterBuilder { Text = $"Please file an issue for the bot if your match seems incorrect." }
            };

            builder.AddField(x =>
            {
                x.Name = $"{eventName}";
                x.Value = $"The event has ended";
                x.IsInline = false;
            });

            var ss = u.SecretSanta as SocketGuildUser;
            builder.AddField(x =>
            {
                x.Name = $"You were matched with";
                x.Value = $"{ss.Nickname} ({ss.Username})";
                x.IsInline = false;
            });

            if (Context.Guild.IconUrl != null)
                builder.WithThumbnailUrl(Context.Guild.IconUrl);

            return builder.Build();
        }

        #endregion
    }
}
