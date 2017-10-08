﻿using NoeSbot.Database;
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
    public class NotifyService : INotifyService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<NotifyService> _logger;

        public NotifyService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<NotifyService>();
        }

        #region Notify

        public async Task<bool> AddNotifyItem(long guildId, long userId, string name, string value, string logo, int type)
        {
            try
            {
                var existing = await _context.NotifyItemEntities.Include(x => x.Users).Include(x => x.Roles).Where(x => x.GuildId == guildId && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Type == type).SingleOrDefaultAsync();
                if (existing != null)
                {
                    var existingItem = existing.Users.Where(x => x.UserId == userId).SingleOrDefault();
                    if (existingItem == null) {
                        existing.Users.Add(new NotifyItem.NotifyUser
                        {
                            UserId = userId
                        });
                    }
                }
                else
                {
                    _context.NotifyItemEntities.Add(new NotifyItem
                    {
                        GuildId = guildId,
                        Logo = logo,
                        Name = name,
                        Type = type,
                        Value = value,
                        Users = new List<NotifyItem.NotifyUser>()
                        {
                            new NotifyItem.NotifyUser
                            {
                                UserId = userId
                            }
                        },
                        Roles = new List<NotifyItem.NotifyRole>()
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Notify Item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveNotifyItem(long guildId, string name, int type)
        {
            try
            {
                var existing = await _context.NotifyItemEntities.Include(x => x.Users).Include(x => x.Roles).Where(x => x.GuildId == guildId && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Type == type).SingleOrDefaultAsync();
                if (existing != null)
                {
                    _context.NotifyItemEntities.Remove(existing);

                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Notify Item: {ex.Message}");
                return false;
            }
        }

        public async Task<List<NotifyItem>> RetrieveAllNotifysAsync()
        {
            try
            {
                var existing = await _context.NotifyItemEntities.Include(x => x.Users).Include(x => x.Roles).ToListAsync();
                if (existing == null)
                    return new List<NotifyItem>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Notify items: {ex.Message}");
                return new List<NotifyItem>();
            }
        }

        public async Task<NotifyItem> RetrieveNotifyAsync(long guildId, string name, int type)
        {
            try
            {
                var existing = await _context.NotifyItemEntities.Include(x => x.Users).Include(x => x.Roles).Where(x => x.GuildId == guildId && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Type == type).SingleOrDefaultAsync();
                if (existing == null)
                    return new NotifyItem();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Notify items: {ex.Message}");
                return new NotifyItem();
            }
        }

        #endregion
    }
}