using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Modules
{
    public class CommonModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;

        public CommonModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        [Command("say")]
        [Alias("echo")]
        [Summary("Echos the provided input")]
        public async Task Say([Remainder] string input)
        {
            await ReplyAsync(input);
        }

        [Command("iswiklasapussy")]
        [Summary("Echos the provided input")]
        public async Task Wiklas([Remainder] string input)
        {
            await ReplyAsync("Yes wiklas is a pussy.");
        }

        [Command("testtime")]
        [Summary("Testing the time input")]
        public async Task TestTime([Remainder] string input)
        {
            var seconds = CommonHelper.GetTimeInSeconds(input);
            
            await ReplyAsync(CommonHelper.GetTimeString(seconds));
        }
    }
}
