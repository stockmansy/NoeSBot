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
    public class ProfileService : IProfileService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ConfigurationService> _logger;

        public ProfileService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ConfigurationService>();
        }

        #region Config
        
        public async Task<bool> AddProfileItem(long guildId, long userId, int profileItemTypeId, string value)
        {
            try
            {
                var existing = await _context.ProfileEntities.Include(x => x.Items).Where(x => x.GuildId == guildId && x.UserId == userId).SingleOrDefaultAsync();
                if (existing != null) {
                    var existingItem = existing.Items.Where(x => x.ProfileItemTypeId == profileItemTypeId).SingleOrDefault();
                    if (existingItem != null)
                    {
                        existingItem.Value = value;
                        _context.ProfileItemEntities.Update(existingItem);

                        await _context.SaveChangesAsync();
                    } else
                    {
                        existing.Items.Add(new ProfileItem
                        {
                            ProfileId = existing.ProfileId,
                            ProfileItemTypeId = profileItemTypeId,
                            Value = value
                        });
                    }
                }
                else
                {
                    _context.ProfileEntities.Add(new Profile
                    {
                        GuildId = guildId,
                        UserId = userId,
                        Items = new List<ProfileItem>
                        {
                            new ProfileItem
                            {
                                ProfileItemTypeId = profileItemTypeId,
                                Value = value
                            }
                        }
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Configuration Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveProfileItem(long guildId, long userId, int profileItemTypeId)
        {
            try
            {
                var existing = await _context.ProfileEntities.Include(x => x.Items).Where(x => x.GuildId == guildId && x.UserId == userId).SingleOrDefaultAsync();
                if (existing != null)
                {
                    var existingItem = existing.Items.Where(x => x.ProfileItemTypeId == profileItemTypeId).SingleOrDefault();
                    if (existingItem != null)
                    {
                        _context.ProfileItemEntities.Remove(existingItem);

                        await _context.SaveChangesAsync();
                    }
                }
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Configuration Item: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Profile>> RetrieveAllProfilesAsync()
        {
            try
            {
                var existing = await _context.ProfileEntities.Include(x => x.Items).ToListAsync();
                if (existing == null)
                    return new List<Profile>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Profile items: {ex.Message}");
                return new List<Profile>();
            }
        }

        public async Task<Profile> RetrieveProfileAsync(long guildId, long userId)
        {
            try
            {
                var existing = await _context.ProfileEntities.Include(x => x.Items).Where(x => x.GuildId == guildId && x.UserId == userId).SingleOrDefaultAsync();
                if (existing == null)
                    return new Profile();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Profile items: {ex.Message}");
                return new Profile();
            }
        }

        #endregion
    }
}
