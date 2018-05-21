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
    public interface ICustomCommandService
    {
        Task<bool> SaveCustomPunishCommandAsync(string commandName, long guildId, long userId, int durationInSec, string reason);

        Task<bool> SaveCustomUnpunishCommandAsync(string commandName, long guildId, long userId);

        Task<bool> RemoveCustomCommandAsync(string commandName);

        Task<IEnumerable<CustomCommand>> RetrieveAllCustomCommandsAsync(long guildId);
    }
}
