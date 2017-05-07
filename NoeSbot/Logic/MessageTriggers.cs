using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NoeSbot.Database;
using NoeSbot.Database.Services;
using NoeSbot.Database.Models;

namespace NoeSbot.Logic
{
    public class MessageTriggers
    {
        private readonly IMessageTriggerService _database;
        private CommandContext _context;
        private IDependencyMap _map;

        public MessageTriggers(CommandContext context, IDependencyMap map)
        {
            _context = context;
            _map = map;
            _database = map.Get<IMessageTriggerService>();
        }

        public async Task Process()
        {
            List<MessageTrigger> triggerList = await _database.RetriveAllMessageTriggers((long)_context.Guild.Id);

            foreach(MessageTrigger trig in triggerList)
            {
                if (Regex.Matches(_context.Message.Content.ToLower(), @"(\s|^)" + trig.Trigger + @"(\s|$)").Count > 0)
                {
                    await _context.Message.Channel.SendMessageAsync(trig.Message);
                }
            }
        }
    }
}
