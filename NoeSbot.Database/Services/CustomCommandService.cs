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
using Newtonsoft.Json;
using NoeSbot.Helpers;

namespace NoeSbot.Database.Services
{
    public class CustomCommandService : ICustomCommandService
    {
        private readonly DatabaseContext _context;

        public CustomCommandService(DatabaseContext context)
        {
            _context = context;
        }

        #region Custom Punish Command

        public async Task<bool> SaveCustomPunishCommandAsync(string commandName, long guildId, long userId, int durationInSec, string reason)
        {
            try
            {
                var existing = await _context.CustomCommandEntities.AsAsyncEnumerable().Where(x => x.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)).SingleOrDefaultAsync();
                if (existing != null)
                    _context.CustomCommandEntities.Remove(existing);

                var cus = new CustomPunishCommand
                {
                    UserId = userId,
                    Duration = durationInSec,
                    Reason = reason
                };

                _context.CustomCommandEntities.Add(new CustomCommand
                {
                    GuildId = guildId,
                    Name = commandName,
                    Type = CustomCommand.CustomCommandType.Punish,
                    Value = JsonConvert.SerializeObject(cus)
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Save Custom Punish Command: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Custom Unpunish Command

        public async Task<bool> SaveCustomUnpunishCommandAsync(string commandName, long guildId, long userId)
        {
            try
            {
                var existing = await _context.CustomCommandEntities.AsAsyncEnumerable().Where(x => x.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)).SingleOrDefaultAsync();
                if (existing != null)
                    _context.CustomCommandEntities.Remove(existing);

                var cus = new CustomUnpunishCommand
                {
                    UserId = userId
                };

                _context.CustomCommandEntities.Add(new CustomCommand
                {
                    GuildId = guildId,
                    Name = commandName,
                    Type = CustomCommand.CustomCommandType.Unpunish,
                    Value = JsonConvert.SerializeObject(cus)
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Save Custom Punish Command: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region General

        public async Task<bool> RemoveCustomCommandAsync(string commandName)
        {
            try
            {
                var existing = await _context.CustomCommandEntities.AsAsyncEnumerable().Where(x => x.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)).SingleOrDefaultAsync();
                if (existing != null)
                    _context.CustomCommandEntities.Remove(existing);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Remove Custom Command: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<CustomCommand>> RetrieveAllCustomCommandsAsync(long guildId)
        {
            try
            {
                var existing = await _context.CustomCommandEntities.AsAsyncEnumerable().Where(x => x.GuildId == guildId).ToListAsync();
                if (existing == null)
                    return new List<CustomCommand>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Retrieve All Custom Commands: {ex.Message}");
                return new List<CustomCommand>();
            }
        }

        #endregion
    }
}