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
    public interface IConfigurationService
    {
        Task<bool> AddConfigurationItem(long guildId, int configTypeId, string value);
        Task<bool> RemoveConfigurationItem(long guildId, int configTypeId, string value);
        Task<bool> SaveConfigurationItem(long guildId, int configTypeId, string value);
        Task<List<Config>> RetrieveAllConfigurationsAsync();
        Task<List<Config>> RetrieveAllOfTypeConfigurationsAsync(int configTypeId);
    }
}
