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
using NoeSbot.Resources;

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

        #region Say

        [Command(Labels.Common_Say_Command)]
        [Alias(Labels.Common_Say_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Say()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Common_Say_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Common_Say_Command)]
        [Alias(Labels.Common_Say_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task Say([Remainder] string input)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(input);
        }

        #endregion

        #region SayTTS

        [Command(Labels.Common_SayTTS_Command)]
        [Alias(Labels.Common_SayTTS_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SayTTS()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Common_SayTTS_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Common_SayTTS_Command)]
        [Alias(Labels.Common_SayTTS_Alias_1)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SayTTS([Remainder] string input)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(input, true);
        }

        #endregion

        [Command(Labels.Common_Info_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
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

        #region Private

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        #endregion
    }
}
