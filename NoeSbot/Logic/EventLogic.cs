using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoeSbot.Converters;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Extensions;
using NoeSbot.Helpers;
using NoeSbot.Models;
using NoeSbot.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class EventLogic
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationService _configurationService;
        private readonly IEventService _eventService;
        private readonly IHttpService _httpService;
        private readonly LimitedDictionary<ulong, EventItem> _eventMessages;

        public EventLogic(DiscordSocketClient client, IConfigurationService configurationService, IEventService eventService, IHttpService httpService)
        {
            _client = client;
            _configurationService = configurationService;
            _httpService = httpService;
            _eventService = eventService;

            _eventMessages = new LimitedDictionary<ulong, EventItem>();

            client.ReactionAdded += OnReactionAdded;
        }

        public async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null || !reaction.User.IsSpecified)
                return;

            var userAdjusting = reaction.User.Value;

            if (!userAdjusting.IsBot && !userAdjusting.IsWebhook)
            {
                var guildId = (long)((userAdjusting as SocketGuildUser).Guild.Id);
                var success = _eventMessages.TryGetValue(message.Id, out EventItem item);
                if (!success)
                {
                    var embed = message.Embeds.FirstOrDefault();
                    if (embed == null || !embed.Footer.HasValue)
                        return;

                    var search = "{u:";
                    var indexOf = embed.Footer.Value.Text.IndexOf(search);
                    if (indexOf <= 0)
                        return;

                    var sub = embed.Footer.Value.Text.Substring(indexOf + search.Length);
                    var nIndexOf = sub.IndexOf("}");

                    if (nIndexOf <= 0)
                        return;

                    var uniqueidentifier = sub.Substring(0, nIndexOf);
                    var eventitem = await _eventService.RetrieveEventAsync(guildId, uniqueidentifier);
                    if (eventitem == null)
                        return;

                    item = new EventItem
                    {
                        Id = eventitem.EventItemId,
                        Name = eventitem.Name,
                        UniqueIdentifier = eventitem.UniqueIdentifier,
                        Date = eventitem.Date,
                        MatchDate = eventitem.MatchDate,
                        Type = (EventEmum)Enum.Parse(typeof(EventEmum), eventitem.Type.ToString())
                    };
                }

                var name = reaction.Emote.Name;

                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                await Task.Run(() => RunParticipantLogic(name, guildId, channel, userAdjusting, item)); //TODO Adjust
            }
        }

        public void AddEvent(ulong key, EventItem item)
        {
            _eventMessages.Add(key, item);
        }


        #region Private

        private async void RunParticipantLogic(string iconname, long guildId, ISocketMessageChannel channel, IUser userAdjusting, EventItem item)
        {
            try
            {
                if (iconname.Equals(IconHelper.Bell))
                {
                    var added = await _eventService.CheckIfAlreadyAParticipant(guildId, (long)userAdjusting.Id, item.UniqueIdentifier);
                    if (!added)
                    {
                        var addSuccess = await _eventService.AddEventParticipant(guildId, (long)userAdjusting.Id, item.UniqueIdentifier);
                        if (addSuccess)
                            await userAdjusting.SendMessageAsync("", false, GetAddParticipantEmbed(item));
                        else
                            await userAdjusting.SendMessageAsync("Something went wrong trying to add you to the participant list");
                    }
                    else
                    {
                        await userAdjusting.SendMessageAsync("You are already part of this event");
                    }
                }
                else if (iconname.Equals(IconHelper.BellStop))
                {
                    var added = await _eventService.CheckIfAlreadyAParticipant(guildId, (long)userAdjusting.Id, item.UniqueIdentifier);
                    if (added)
                    {
                        var removeSuccess = await _eventService.RemoveEventParticipant(guildId, (long)userAdjusting.Id, item.UniqueIdentifier);
                        if (removeSuccess)
                            await userAdjusting.SendMessageAsync("Successfully removed you from the participant list");
                        else
                            await userAdjusting.SendMessageAsync("Something went wrong trying to remove you to the participant list");
                    }
                    else
                    {
                        await userAdjusting.SendMessageAsync("You weren't part of this event");
                    }
                }
                else if (iconname.Equals(IconHelper.ClipBoard))
                {
                    var eventItem = await _eventService.RetrieveEventAsync(guildId, item.UniqueIdentifier);
                    if (eventItem != null)
                    {
                        var partsTasks = eventItem.Participants.Select(async x =>
                        {
                            return await channel.GetUserAsync((ulong)x.UserId);
                        });
                        var parts = await Task.WhenAll(partsTasks);

                        var embed = GetParticipantsEmbed(parts, item);
                        await userAdjusting.SendMessageAsync("", false, embed);
                    }
                }
            }
            catch { }
        }

        private Embed GetParticipantsEmbed(IEnumerable<IUser> participants, EventItem item)
        {
            var builder = new EmbedBuilder()
            {
                Color = Color.Red,
                Description = $"A list of Participants for the event: {item.Name}"
            };

            if (!participants.Any())
            {
                builder.AddField(x =>
                {
                    x.Name = $"Participants";
                    x.Value = "None";
                    x.IsInline = false;
                });
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var participant in participants)
                    sb.AppendLine($"{participant.Username}");

                builder.AddField(x =>
                {
                    x.Name = $"Participants";
                    x.Value = sb.ToString();
                    x.IsInline = false;
                });
            }

            return builder.Build();
        }

        private Embed GetAddParticipantEmbed(EventItem item)
        {
            var builder = new EmbedBuilder()
            {
                Color = Color.Red,
                Description = $"Successfully added you to the participant list{Environment.NewLine}{Environment.NewLine}Information:{Environment.NewLine}"
            };

            builder.AddField(x =>
            {
                x.Name = $"{item.Name}";
                x.Value = $"The event will end on {item.Date.ToLocalTime().ToString("dd-MM-yyyy hh:mm")}";
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

            return builder.Build();
        }

        #endregion
    }
}
