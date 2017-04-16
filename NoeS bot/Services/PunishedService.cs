using NoeSbot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NoeSbot.Services
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
                var existing = await _context.PunishedEntities.ToListAsync();
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
    }
}
