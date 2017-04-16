using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace NoeSbot.Services
{
    public interface IPunishedService
    {
        Task<bool> SavePunishedAsync(long userId, DateTime timeOfPunishment, int durationInSec, string reason);

        Task<bool> RemovePunishedAsync(long userId);

        Task<bool> RemovePunishedAsync();

        Task<List<Punished>> RetrieveAllPunishedAsync();
    }
}
