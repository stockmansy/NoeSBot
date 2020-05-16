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
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ProfileService>();
        }

        #region ProfileItem
        
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

        #region Profile background
        
        public async Task<bool> AddOrUpdateProfileBackground(long guildId, ProfileBackground.ProfileBackgroundSetting setting, string value, long userId, IEnumerable<string> aliases = null)
        {
            try
            {
                ProfileBackground existing = null;
                if (setting.Equals(ProfileBackground.ProfileBackgroundSetting.Custom))
                {
                    existing = await _context.ProfileBackgroundEntities.Include(zx => zx.Aliases).Where(x => x.UserId == userId && x.ProfileBackgroundSettingId == ProfileBackground.ProfileBackgroundSetting.Custom && x.GuildId == guildId).SingleOrDefaultAsync();
                } else // Game
                {
                    var allBgs = await _context.ProfileBackgroundEntities.Include(zx => zx.Aliases).ToListAsync();
                    if (aliases == null)
                        throw new Exception("No alliases provided");

                    var matches = aliases.SelectMany(x =>
                    {
                        return allBgs.Select(y =>
                        {
                            if (y.Aliases.Select(z => z.Alias).ToList().Contains(x.Replace(" ", "")))
                                return y;
                            return null;
                        });
                    }).Where(x => x != null);

                    existing = matches.FirstOrDefault();
                }

                var newAliases = new List<ProfileBackground.ProfileBackgroundAlias>();

                if (existing == null)
                {
                    if (aliases != null)
                        aliases.ForEach(a =>
                        {
                            newAliases.Add(new ProfileBackground.ProfileBackgroundAlias
                            {
                                Alias = a.Replace(" ", "")
                            });
                        });

                    await _context.ProfileBackgroundEntities.AddAsync(new ProfileBackground
                    {
                        GuildId = guildId,
                        Aliases = newAliases,
                        UserId = userId,
                        ProfileBackgroundSettingId = setting,
                        Value = value
                    });
                } else
                {
                    if (aliases != null) { 
                        aliases.ForEach(a =>
                        {
                            if (existing.Aliases == null)
                                existing.Aliases = new List<ProfileBackground.ProfileBackgroundAlias>();

                            var exist = existing.Aliases.Where(x => x.Alias.Replace(" ", "").Equals(a, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                            if (exist == null)
                            {
                                existing.Aliases.Add(new ProfileBackground.ProfileBackgroundAlias
                                {
                                    Alias = a.Replace(" ", "")
                                });
                            }
                        });
                    }
                    
                    existing.UserId = userId;
                    existing.Value = value;
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Add or Update Profile Background: {ex.Message}");
                return false;
            }
        }

        public async Task<ProfileBackground> RetrieveProfileBackground(long guildId, long? userId, string alias = null)
        {
            if (alias == null && userId == null)
                throw new Exception("Invalid request");

            try
            {
               return await _context.ProfileBackgroundEntities.Include(zx => zx.Aliases).Where(x => x.GuildId == guildId && ((x.UserId == userId && x.ProfileBackgroundSettingId == ProfileBackground.ProfileBackgroundSetting.Custom) || (x.Aliases.Select(al => al.Alias).Contains(alias.Replace(" ", ""))))).FirstOrDefaultAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve Profile Background: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
