using NoeSbot.Database.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using NoeSbot.Helpers;

namespace NoeSbot.Database.Services
{
    public class MessageTriggerService : IMessageTriggerService
    {
        private readonly DatabaseContext _context;

        public MessageTriggerService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveMessageTrigger(string trigger, string message, bool tts, long server)
        {
            try
            {
                var existing = await _context.MessageTriggerEntities.Where(x => x.Trigger == trigger && x.Server == server).SingleOrDefaultAsync();
                if (existing != null)
                    _context.MessageTriggerEntities.Remove(existing);

                _context.MessageTriggerEntities.Add(new MessageTrigger
                {
                    Trigger = trigger,
                    Message = message,
                    Server = server,
                    Tts = tts
                });

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Save MessageTrigger: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMessageTrigger(string trigger, long server)
        {
            try
            {
                var existing = await _context.MessageTriggerEntities.Where(x => x.Trigger == trigger && x.Server == server).SingleOrDefaultAsync();
                if (existing != null)
                    _context.MessageTriggerEntities.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Delete MessageTrigger: {ex.Message}");
                return false;
            }
        }

        public async Task<List<MessageTrigger>> RetriveAllMessageTriggers(long server)
        {
            try
            {
                var existing = await _context.MessageTriggerEntities.Where(x => x.Server == server).ToListAsync();
                if (existing == null)
                    return new List<MessageTrigger>();

                return existing;
            }
            catch (DbUpdateException ex)
            {
                LogHelper.LogError($"Error in Retrieve All MessageTrigger: {ex.Message}");
                return new List<MessageTrigger>();
            }
        }

    }
}
