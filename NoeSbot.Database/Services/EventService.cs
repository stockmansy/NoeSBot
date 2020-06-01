using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NoeSbot.Database.Models;
using NoeSbot.Helpers;

namespace NoeSbot.Database.Services
{
    public class EventService : IEventService
    {
        private readonly DatabaseContext _context;

        public EventService(DatabaseContext context)
        {
            _context = context;
        }

        #region Event

        public async Task<bool> AddEventItem(long guildId, long organiserId, string uniqueidentifier, string name, string description, int type, DateTime date, DateTime? matchdate)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Active)
                                        .SingleOrDefaultAsync();
                if (existing != null)
                    throw new Exception("The unique identifier is already in use");

                _context.EventItemEntities.Add(new EventItem
                {
                    GuildId = guildId,
                    UniqueIdentifier = uniqueidentifier,
                    Name = name,
                    Description = description,
                    Type = type,
                    Date = date,
                    MatchDate = matchdate,
                    Active = true,
                    Organisers = new List<EventItem.Organiser>()
                    {
                        new EventItem.Organiser
                        {
                            UserId = organiserId
                        }
                    },
                    Participants = new List<EventItem.Participant>()
                    {
                        new EventItem.Participant
                        {
                            UserId = organiserId
                        }
                    },
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Save Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateEventItem(long guildId, long organiserId, string uniqueidentifier, string name, string description, DateTime date, DateTime? matchdate)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Organisers.Where(y => y.UserId == organiserId).Any()
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    existing.Name = name;
                    existing.Description = description;
                    existing.Date = date;
                    existing.MatchDate = matchdate;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in updating Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddEventOrganiser(long guildId, long organiserId, long newOrganiserId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Organisers.Where(y => y.UserId == organiserId).Any()
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    var organiser = existing.Organisers.Where(x => x.UserId == organiserId).SingleOrDefault();
                    if (organiser == null)
                    {
                        existing.Organisers.Add(new EventItem.Organiser
                        {
                            UserId = newOrganiserId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in updating Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddEventParticipant(long guildId, long participantId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    var participant = existing.Participants.Where(x => x.UserId == participantId).SingleOrDefault();
                    if (participant == null)
                    {
                        existing.Participants.Add(new EventItem.Participant
                        {
                            UserId = participantId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in updating Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DisableEventItem(long guildId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    existing.Active = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in updating Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckIfAlreadyAnOrganiser(long guildId, long organiserId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Organisers.Where(y => y.UserId == organiserId).Any()
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    return true;
                }

                return false;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in checking if there is already an organiser with that id: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckIfAlreadyAParticipant(long guildId, long participantId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Participants.Where(y => y.UserId == participantId).Any()
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    return true;
                }

                return false;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in checking if there is already a participant with that id: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveEventOrganiser(long guildId, long organiserId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Organisers.Where(y => y.UserId == organiserId).Any()
                                                && x.Active)
                                        .SingleOrDefaultAsync();

                if (existing != null)
                {
                    var organiser = existing.Organisers.Where(x => x.UserId == organiserId).SingleOrDefault();
                    if (organiser != null)
                    {
                        existing.Organisers.Remove(organiser);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in updating Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveEventParticipant(long guildId, long participantId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.GuildId == guildId
                                                && x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase)
                                                && x.Participants.Where(y => y.UserId == participantId).Any()
                                                && x.Active)
                                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    var participant = existing.Participants.Where(x => x.UserId == participantId).SingleOrDefault();
                    if (participant != null)
                    {
                        existing.Participants.Remove(participant);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in updating Event Item: {ex.Message}");
                return false;
            }
        }

        public async Task<List<EventItem>> RetrieveAllEventsAsync()
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .ToListAsync();
                if (existing == null)
                    return new List<EventItem>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in retrieving all event items: {ex.Message}");
                return new List<EventItem>();
            }
        }

        public async Task<EventItem> RetrieveEventAsync(long guildId, string uniqueidentifier)
        {
            try
            {
                var existing = await _context.EventItemEntities
                                        .Include(x => x.Participants)
                                        .Include(x => x.Organisers)
                                        .AsAsyncEnumerable()
                                        .Where(x => x.UniqueIdentifier.Equals(uniqueidentifier, StringComparison.OrdinalIgnoreCase) && x.GuildId == guildId)
                                        .SingleOrDefaultAsync();
                if (existing == null)
                    return new EventItem();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in retrieving an event: {ex.Message}");
                return new EventItem();
            }
        }

        #endregion
    }
}
