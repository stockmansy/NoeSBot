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
    public interface IPunishedService
    {
        Task<bool> SavePunishedAsync(long userId, DateTime timeOfPunishment, int durationInSec, string reason);

        Task<bool> RemovePunishedAsync(long userId);

        Task<bool> RemovePunishedAsync();

        Task<List<Punished>> RetrieveAllPunishedAsync();

        Task<bool> SaveCustomPunishedAsync(long userId, string reason, string delayreason);

        Task<bool> RemoveCustomPunishedAsync(long userId, int index);

        Task<bool> RemoveCustomPunishedAsync(long userId);

        Task<List<CustomPunished>> RetrieveAllCustomPunishedAsync(long userId);
    }
}
