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

namespace NoeSbot.Database.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ConfigurationService> _logger;
        
        public ConfigurationService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ConfigurationService>();
        }

        #region Config

        public async Task<bool> AddConfigurationItem(long guildId, int configTypeId, string value)
        {
            try
            {
                var existing = await _context.ConfigurationEntities.AsQueryable().Where(x => x.GuildId == guildId && x.ConfigurationTypeId == configTypeId && x.Value.Equals(value, StringComparison.OrdinalIgnoreCase)).SingleOrDefaultAsync();
                if (existing != null)
                    _context.ConfigurationEntities.Remove(existing);

                _context.ConfigurationEntities.Add(new Config
                {
                    GuildId = guildId,
                    ConfigurationTypeId = configTypeId,
                    Value = value
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Configuration Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveConfigurationItem(long guildId, int configTypeId, string value)
        {
            try
            {
                var existing = await _context.ConfigurationEntities.Where(x => x.GuildId == guildId && x.ConfigurationTypeId == configTypeId && x.Value.Equals(value, StringComparison.OrdinalIgnoreCase)).SingleOrDefaultAsync();
                if (existing != null)
                    _context.ConfigurationEntities.Remove(existing);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Configuration Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveConfigurationItem(long guildId, int configTypeId, string value)
        {
            try
            {
                var existing = await _context.ConfigurationEntities.Where(x => x.GuildId == guildId && x.ConfigurationTypeId == configTypeId).SingleOrDefaultAsync();
                if (existing != null)
                    _context.ConfigurationEntities.Remove(existing);

                _context.ConfigurationEntities.Add(new Config
                {
                    GuildId = guildId,
                    ConfigurationTypeId = configTypeId,
                    Value = value
                });
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Configuration Item: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Config>> RetrieveAllConfigurationsAsync()
        {
            try
            {
                var existing = await _context.ConfigurationEntities.AsAsyncEnumerable().ToListAsync();
                if (existing == null)
                    return new List<Config>();
                
                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Config items: {ex.Message}");
                return new List<Config>();
            }
        }

        public async Task<List<Config>> RetrieveAllOfTypeConfigurationsAsync(int configTypeId)
        {
            try
            {
                var existing = await _context.ConfigurationEntities.Where(x => x.ConfigurationTypeId == configTypeId).ToListAsync();
                if (existing == null)
                    return new List<Config>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Config items by type: {ex.Message}");
                return new List<Config>();
            }
        }

        #endregion
    }
}
