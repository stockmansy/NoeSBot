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
    public class PunishedService : IPunishedService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<PunishedService> _logger;
        
        public PunishedService(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<PunishedService>();
        }

        #region Punished

        public async Task<bool> SavePunishedAsync(long userId, DateTime timeOfPunishment, int durationInSec, string reason)
        {
            try
            {
                var existing = await _context.PunishedEntities.Where(x => x.UserId == userId).SingleOrDefaultAsync();
                if (existing != null)
                    _context.PunishedEntities.Remove(existing);

                _context.PunishedEntities.Add(new Punished
                {
                    UserId = userId,
                    TimeOfPunishment = timeOfPunishment,
                    Duration = durationInSec,
                    Reason = reason
                });
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Punished: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemovePunishedAsync(long userId)
        {
            try
            {
                var existing = await _context.PunishedEntities.Where(x => x.UserId == userId).SingleOrDefaultAsync();
                if (existing != null)
                    _context.PunishedEntities.Remove(existing);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Remove Punished: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemovePunishedAsync()
        {
            try
            {
                _context.PunishedEntities.RemoveRange(_context.PunishedEntities);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Remove All Punished: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Punished>> RetrieveAllPunishedAsync()
        {
            try
            {
                var existing = await _context.PunishedEntities.AsAsyncEnumerable().ToListAsync();
                if (existing == null)
                    return new List<Punished>();
                
                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Punished: {ex.Message}");
                return new List<Punished>();
            }
        }

        #endregion

        #region Custom Punished

        public async Task<bool> SaveCustomPunishedAsync(long userId, string reason, string delayreason)
        {
            try
            {
                _context.CustomPunishedEntities.Add(new CustomPunished
                {
                    UserId = userId,
                    Reason = reason,
                    DelayMessage = delayreason
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Save Custom Punished: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveCustomPunishedAsync(long userId, int index)
        {
            try
            {
                var existing = await _context.CustomPunishedEntities.Where(x => x.UserId == userId).ToListAsync();
                if (existing == null)
                    throw new Exception("The existing custom rule was not found");

                var item = existing[index];

                if (item != null)
                    _context.CustomPunishedEntities.Remove(item);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Remove Custom Punished: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveCustomPunishedAsync(long userId)
        {
            try
            {
                _context.CustomPunishedEntities.RemoveRange(_context.CustomPunishedEntities.Where(x => x.UserId == userId));

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Remove All Custom Punished: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CustomPunished>> RetrieveAllCustomPunishedAsync(long userId)
        {
            try
            {
                var existing = await _context.CustomPunishedEntities.Where(x => x.UserId == userId).ToListAsync();
                if (existing == null)
                    return new List<CustomPunished>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error in Retrieve All Custom Punished: {ex.Message}");
                return new List<CustomPunished>();
            }
        }

        #endregion
    }
}
