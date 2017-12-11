using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using NoeSbot.Database.Models;

namespace NoeSbot.Database.Services
{
    public interface IEventService
    {
        Task<bool> AddEventItem(long guildId, long organiserId, string uniqueidentifier, string name, string description, int type, DateTime date, DateTime? matchdate);

        Task<bool> UpdateEventItem(long guildId, long organiserId, string uniqueidentifier, string name, string description, DateTime date, DateTime? matchdate);

        Task<bool> AddEventOrganiser(long guildId, long organiserId, long newOrganiserId, string uniqueidentifier);

        Task<bool> AddEventParticipant(long guildId, long participantId, string uniqueidentifier);

        Task<bool> DisableEventItem(long guildId, string uniqueidentifier);

        Task<bool> CheckIfAlreadyAnOrganiser(long guildId, long organiserId, string uniqueidentifier);

        Task<bool> CheckIfAlreadyAParticipant(long guildId, long participantId, string uniqueidentifier);

        Task<bool> RemoveEventOrganiser(long guildId, long organiserId, string uniqueidentifier);

        Task<bool> RemoveEventParticipant(long guildId, long participantId, string uniqueidentifier);

        Task<List<EventItem>> RetrieveAllEventsAsync();

        Task<EventItem> RetrieveEventAsync(long guildId, string uniqueidentifier);
    }
}