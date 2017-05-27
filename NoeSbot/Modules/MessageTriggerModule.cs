using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Database;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Database.Services;
using NoeSbot.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.MessageTrigger)]
    public class MessageTriggerModule : ModuleBase
    {
        private readonly IMessageTriggerService _database;
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;

        public MessageTriggerModule(IMessageTriggerService database, DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _database = database;
            _client = client;
            _cache = memoryCache;
        }

        [Command("addtrigger")]
        [Alias("trigger")]
        [Summary("Add a trigger in messages")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task AddTrigger([Summary("The trigger")] string trig,
                                        [Summary("The message triggered")] string mess,
                                        [Summary("Optional tts")] bool tts = false)
        {
            var success = await _database.SaveMessageTrigger(trig.ToLower(), mess, tts, (long)Context.Guild.Id);
            if (success)
                await ReplyAsync("Trigger for " + trig.ToLower() + " successfully added");
            else
                await ReplyAsync("Something went wrong. Trigger not saved.");
        }

        [Command("deletetrigger")]
        [Alias("deltrigger", "removetrigger")]
        [Summary("Deletes a trigger in messages")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task AddTrigger([Summary("The trigger")] string trig)
        {
            var success = await _database.DeleteMessageTrigger(trig.ToLower(), (long)Context.Guild.Id);
            if (success)
                await ReplyAsync("Trigger for " + trig.ToLower() + " successfully removed");
            else
                await ReplyAsync("Trigger does not exist or something else went wrong.");
        }
    }
}
