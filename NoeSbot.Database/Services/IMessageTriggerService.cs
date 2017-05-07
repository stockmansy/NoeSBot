using NoeSbot.Database;
using NoeSbot.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Database.Services
{
    public interface IMessageTriggerService
    {
        Task<bool> SaveMessageTrigger(string trigger, string message, long server);

        Task<List<MessageTrigger>> RetriveAllMessageTriggers(long server);
    }
}
