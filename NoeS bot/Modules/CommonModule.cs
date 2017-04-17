using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        [Summary("Make the bot say ...")]
        public async Task Say([Remainder] string input)
        {

            await ReplyAsync(input);
        }

        [Command("iswiklasapussy")]
        [Summary("Check if Wiklas is a pussy")]
        public async Task Wiklas([Remainder] string input)
        {
            await ReplyAsync("Yes wiklas is a pussy.");
        }

        [Command("info")]
        [Summary("Get info about the bot")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}
