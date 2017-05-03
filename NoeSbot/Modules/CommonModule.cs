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

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Common)]
    public class CommonModule : ModuleBase
    {
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;

        public CommonModule(DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _cache = memoryCache;
        }

        #region Help text

        [Command("say")]
        [Alias("echo")]
        [Summary("Make the bot say ...")]
        public async Task Say()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 parameter: Text");
            builder.AppendLine("This command will replace your message by a message by the bot");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        #endregion

        [Command("say")]
        [Alias("echo")]
        [Summary("Make the bot say ...")]
        public async Task Say([Remainder] string input)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(input);
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
