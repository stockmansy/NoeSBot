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
using NoeSbot.Database.ViewModels;

namespace NoeSbot.Database.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ConfigurationService> _logger;

        public ActivityLogService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ConfigurationService>();
        }

        #region ActivityLogs

        public async Task<bool> AddActivityLog(long guildId, long userId, long channelId, string command, string log)
        {
            try
            {
                var activityLog = await GetMainGuildActivityLog(guildId);

                activityLog.Logs.Add(new ActivityLog.ActivityLogItem()
                {
                    UserId = userId,
                    ChannelId = channelId,
                    Command = command,
                    Log = log,
                    CreationDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                });

                activityLog.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Add Activity Log: {ex.Message}");
                return false;
            }
        }

        public async Task<ActivityLogVM> RetrieveActivityLog(long guildId, long? userId = null)
        {
            try
            {
                var activityLog = await GetMainGuildActivityLog(guildId);

                var tempLogs = activityLog.Logs.Where(x => x.ModifiedDate >= DateTime.UtcNow.AddMonths(-2));
                if (userId.HasValue)
                    tempLogs = tempLogs.Where(x => x.UserId == userId.Value);
                var logs = tempLogs.Select(x => new ActivityLogVM.ActivityLogVMItem(x.ActivityLogItemId, x.UserId, x.ChannelId, x.Command, x.Log, x.ModifiedDate));
                
                return new ActivityLogVM(activityLog.ActivityLogId, guildId, logs);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Profile items: {ex.Message}");
                return new ActivityLogVM(-1, guildId);
            }
        }

        #endregion

        #region Private

        private async Task<ActivityLog> GetMainGuildActivityLog(long guildId)
        {
            var activityLog = await _context.ActivityLogEntities.Include(x => x.Logs).Where(x => x.GuildId == guildId).SingleOrDefaultAsync();
            if (activityLog == null)
            {
                _context.ActivityLogEntities.Add(new ActivityLog()
                {
                    GuildId = guildId,
                    CreationDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return await GetMainGuildActivityLog(guildId);
            }

            return activityLog;
        }

        #endregion
    }
}
