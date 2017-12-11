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

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Event)]
    public class EventModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private Random _random = new Random();        
        private IEventService _eventService;
        private EventLogic _eventLogic;

        #region Constructor

        public EventModule(DiscordSocketClient client, IMemoryCache memoryCache, IEventService eventService, EventLogic eventLogic)
        {
            _client = client;
            _cache = memoryCache;
            _eventService = eventService;
            _eventLogic = eventLogic;
        }

        #endregion
        

        #region Commands

        #region Start Event

        [Command(Labels.Event_StartEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task StartEvent()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Event_StartEvent_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }


        [Command(Labels.Event_StartEvent_Command)]
        [Alias(Labels.Event_StartEvent_Alias_1)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task StartEvent(string eventtype, string uniqueidentifier, string name, string description, string datetime)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
                            if (matchdate <= DateTime.Now) { 
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
                    var builder = new EmbedBuilder()
                    {
                        Color = user.GetColor(),
                        Description = "You didn't start the event properly, make sure you have a unique id. Refer to the help text"
                    };

                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                }
            }
        }

        #endregion

        #region Update Event

        [Command(Labels.Event_UpdateEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task UpdateEvent()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Event_UpdateEvent_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Event_UpdateEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task UpdateEvent(string uniqueidentifier, string name, string description, string datetime)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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

                    if (matchdate != null) { 
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
        }

        #endregion

        #region Stop Event

        [Command(Labels.Event_StopEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task StopEvent()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;
                await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Event_StopEvent_Command, Configuration.Load(Context.Guild.Id).Prefix, user.GetColor()));
            }
        }

        [Command(Labels.Event_StopEvent_Command)]
        [MinPermissions(AccessLevel.ServerAdmin)]
        public async Task StopEvent(string uniqueidentifier)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;

                try
                {
                    if (await _eventService.DisableEventItem((long)Context.Guild.Id, uniqueidentifier)) {
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
                x.Value = $"The event will end on {item.Date.ToLocalTime().ToString("dd-MM-yyyy hh:mm")}";
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
                    x.Value = $"Secret santa matches will be send out on {item.MatchDate.Value.ToLocalTime().ToString("dd-MM-yyyy hh:mm")}";
                    x.IsInline = false;
                });
            }

            if (Context.Guild.IconUrl != null)
                builder.WithThumbnailUrl(Context.Guild.IconUrl);
            
            var message = await Context.Channel.SendMessageAsync("", false, builder.Build());

            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.Bell));
            await Task.Delay(1250);
            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.BellStop));
            await Task.Delay(1250);
            await message.AddReactionAsync(IconHelper.GetEmote(IconHelper.ClipBoard));

            _eventLogic.AddEvent(message.Id, item);
        }

        #endregion
    }
}
