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
        private IMemoryCache _cache;

        public MessageTriggers(IMessageTriggerService database, IMemoryCache memoryCache)
        {
            _database = database;
            _cache = memoryCache;
        }

        public async Task Process(ICommandContext context)
        {
            if (!_cache.TryGetValue(CacheEnum.MessageTriggers, out IEnumerable<MessageTrigger> triggerList))
            {
                triggerList = await _database.RetriveAllMessageTriggers((long)context.Guild.Id);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(CacheEnum.MessageTriggers, triggerList, cacheEntryOptions);
            }

            foreach(var trig in triggerList)
            {
                if (Regex.Matches(context.Message.Content.ToLower(), @"(\s|^)" + trig.Trigger + @"(\s|$)").Count > 0)
                {
                    await context.Message.Channel.SendMessageAsync(trig.Message, trig.Tts);
                }
            }
        }
    }
}
