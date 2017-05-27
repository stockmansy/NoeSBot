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
    public interface IProfileService
    {
        Task<bool> AddProfileItem(long guildId, long userId, int profileItemTypeId, string value);
        Task<bool> RemoveProfileItem(long guildId, long userId, int profileItemTypeId);
        Task<List<Profile>> RetrieveAllProfilesAsync();
        Task<Profile> RetrieveProfileAsync(long guildId, long userId);
    }
}
