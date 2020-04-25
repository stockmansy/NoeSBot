using System;
using System.Linq;
using NoeSbot.Database.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NoeSbot.Database.Services
{
    public class SerializedItemService : ISerializedItemService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ConfigurationService> _logger;

        public SerializedItemService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ConfigurationService>();
        }

        #region Guild backup

        public async Task<bool> AddGuildBackup(long guildId, object backup)
        {
            try
            {
                await RemoveOldGuildBackup(guildId);
                await SaveSerializedItem(guildId, (int)SerializedItem.SerializedItemType.GuildBackup, backup);

                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Add Guild Backup: {ex.Message}");
                return false;
            }
        }

        private async Task RemoveOldGuildBackup(long guildId)
        {
            var existingOld = await _context.SerializedItemEntities.Where(x => x.GuildId == guildId &&
                                                                               x.Type == (int)SerializedItem.SerializedItemType.GuildBackup &&
                                                                               x.ModifiedDate.Ticks < DateTimeOffset.UtcNow.AddMonths(-1).Ticks)
                                                                   .ToListAsync();
            _context.SerializedItemEntities.RemoveRange(existingOld);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Private

        private async Task SaveSerializedItem(long guildId, int type, object item)
        {
            await _context.SerializedItemEntities.AddAsync(new SerializedItem
            {
                GuildId = guildId,
                Type = type,
                Content = JsonConvert.SerializeObject(item, Formatting.None,
                                                            new JsonSerializerSettings
                                                            {
                                                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                                Formatting = Formatting.None,
                                                                DateFormatString = "yyyy-MM-dd",
                                                                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                                                            }),
                CreationDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow // Something wrong with doing it by default
            });

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
