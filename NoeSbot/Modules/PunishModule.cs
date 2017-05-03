using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using NoeSbot.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Database;
using System.Threading;
using NoeSbot.Database.Services;
using NoeSbot.Database.Models;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Punish)]
    public class PunishModule : ModuleBase
    {
        private readonly IPunishedService _database;
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private bool _runPunishedCheck = false;

        #region Constructor

        public PunishModule(IPunishedService database, DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _database = database;
            _client = client;
            _cache = memoryCache;

            _client.MessageReceived += HandleCommand;
        }

        #endregion

        #region Handlers

        public async Task HandleCommand(SocketMessage messageParam)
        {
            if (!messageParam.Author.IsBot && !messageParam.Author.IsWebhook)
            {
                var allPunished = await GetAllPunishedAsync();
                if (allPunished.Where(x => x.UserId == (long)messageParam.Author.Id).Any())
                {
                    var shouldDelete = await CheckPunishedAsync((SocketGuildUser)messageParam.Author, allPunished);
                    if (shouldDelete)
                        await messageParam.DeleteAsync();
                }
            }
        }

        #endregion

        #region Help text

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user) (Defaults to 5m No reason given)")]
        public async Task AddCustomPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: user");
            builder.AppendLine("2 optional parameters: time (eg 10m), reason");
            builder.AppendLine("Punish a certain user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("unpunish")]
        [Alias("unsilence")]
        [Summary("Unpunish specific user")]
        public async Task UnPunish()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: user");
            builder.AppendLine("Unpunish a certain user");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user) (Defaults to 5m No reason given)")]
        public async Task UnPunishAll()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: input");
            builder.AppendLine("Unpunish all users if the text is 'all'");
            builder.AppendLine("Permissions: Mod minimum");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        #endregion

        #region Commands

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user) (Defaults to 5m No reason given)")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user)
        {
            await Punish(user, "5m", "No reason given");
        }

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user, time) (Defaults to No reason given)")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user,
                                 [Summary("The punish time")]string time)
        {
            await Punish(user, time, "No reason given");
        }

        [Command("punish")]
        [Alias("silence")]
        [Summary("Punish people (param user time reason)")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punish([Summary("The user to be punished")] SocketGuildUser user,
                                 [Summary("The punish time")]string time,
                                 [Remainder, Summary("The punish reason")]string reason)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook && !user.IsBot)
            {
                var durationInSecs = CommonHelper.GetTimeInSeconds(time);
                var success = await _database.SavePunishedAsync((long)user.Id, DateTime.UtcNow, durationInSecs, reason);

                var punishedRoles = GetPunishRoles();
                if (success && punishedRoles != null)
                {
                    foreach (var role in punishedRoles)
                        await user.AddRoleAsync(role);

                    ClearCache();

                    if (!_runPunishedCheck)
                        StartPunishedCheck();

                    var customMsgs = await _database.RetrieveAllCustomPunishedAsync((long)user.Id);
                    if (customMsgs == null || customMsgs.Count() == 0)
                        await Context.Channel.SendFileAsync(Globals.RandomPunishedImage.FullName, $"Successfully punished {user.Mention} ({user.Username}) for {CommonHelper.ToReadableString(TimeSpan.FromSeconds(durationInSecs))}");
                    else
                    {
                        var rnd = new Random();
                        int r = rnd.Next(customMsgs.Count());
                        var randomCustomMsg = customMsgs[r];
                        if (randomCustomMsg == null)
                            await ReplyAsync($"Failed to punish {user.Username}");

                        if (!string.IsNullOrWhiteSpace(randomCustomMsg.DelayMessage))
                        {
                            await ReplyAsync(randomCustomMsg.DelayMessage.GetProcessedString());
                            await Task.Delay(3000);
                        }

                        await ReplyAsync(randomCustomMsg.Reason.GetProcessedString());
                    }
                }
                else
                    await ReplyAsync($"Failed to punish {user.Username}");
            }
        }

        [Command("punished")]
        [Alias("silenced")]
        [Summary("List of the punished users")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task Punished()
        {
            var allPunished = await GetAllPunishedAsync();
            var punishedRoles = GetPunishRoles();
            var count = allPunished.Count();

            var builder = new StringBuilder();
            builder.AppendLine("The following users were punished:");
            builder.AppendLine("```");

            foreach (var pun in allPunished)
            {
                var user = await Context.Guild.GetUserAsync((ulong)pun.UserId);
                var punishTime = CommonHelper.GetTimeString(pun.TimeOfPunishment, pun.Duration);
                if (punishTime.StartsWith("-")) { 
                    await Unpunish(punishedRoles, (SocketGuildUser)user);
                    await StopPunishedCheckAsync();
                    count--;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(user.Nickname))
                        builder.AppendLine($"{user.Nickname} ({user.Username}) is punished for: {punishTime}.");
                    else
                        builder.AppendLine($"{user.Username} is punished for: {punishTime}.");
                }
                    
            }
            
            if (count <= 0)
                builder.AppendLine("None");

            builder.AppendLine("```");

            await ReplyAsync(builder.ToString());
        }

        [Command("unpunish")]
        [Alias("unsilence")]
        [Summary("Unpunish specific user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task UnPunish([Summary("The user to be unpunished")] SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var all = await GetAllPunishedAsync();
                if (all.Where(x => x.UserId == (long)Context.User.Id).Any())
                    return;

                var success = await _database.RemovePunishedAsync((long)user.Id);
                var punishedRoles = GetPunishRoles();

                if (success && punishedRoles != null && user.Id != 106079994945536000)
                {
                    foreach (var role in punishedRoles)
                        await user.RemoveRoleAsync(role);

                    ClearCache();

                    await StopPunishedCheckAsync();

                    await ReplyAsync($"Successfully unpunished {user.Mention} ({user.Username})");
                }
                else
                    await ReplyAsync($"Failed to unpunish {user.Username}");
            }
        }

        [Command("unpunish")]
        [Alias("unsilence")]
        [Summary("Unpunish all users")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task UnPunish([Remainder, Summary("The punish input")]string input)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                if (input.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    var all = await GetAllPunishedAsync();
                    if (all.Where(x => x.UserId == (long)Context.User.Id).Any())
                        return;

                    var success = await _database.RemovePunishedAsync();
                    var punishedRoles = GetPunishRoles();

                    if (success && punishedRoles != null)
                    {
                        foreach (var pun in all)
                        {
                            var iuser = await Context.Guild.GetUserAsync((ulong)pun.UserId);
                            var user = (SocketGuildUser)iuser;

                            foreach (var role in punishedRoles)
                            {
                                await user.RemoveRoleAsync(role);
                            }
                        }

                        ClearCache();

                        await StopPunishedCheckAsync();

                        await ReplyAsync($"Successfully unpunished everybody");
                    }
                    else
                        await ReplyAsync($"Failed to unpunish everybody");
                }
            }
        }

        #endregion

        #region Private

        private void StartPunishedCheck()
        {
            _runPunishedCheck = true;
            DoPunishedCheckTask();
        }

        private async Task StopPunishedCheckAsync()
        {
            var allPunished = await GetAllPunishedAsync();
            if (allPunished.Count() <= 0)
            {
                _runPunishedCheck = false;
            }
        }

        private void DoPunishedCheckTask()
        {
            Task.Run(async () =>
            {
                while (true && _runPunishedCheck)
                {
                    var allPunished = await GetAllPunishedAsync();
                    var punishedRoles = GetPunishRoles();
                    var count = allPunished.Count();

                    if (count <= 0)
                    {
                        _runPunishedCheck = false;
                        return;
                    }

                    foreach (var pun in allPunished)
                    {
                        var user = await Context.Guild.GetUserAsync((ulong)pun.UserId);
                        var shouldStop = await CheckPunishedAsync((SocketGuildUser)user, allPunished);

                        if (!shouldStop && count == 1)
                            _runPunishedCheck = false;
                    }

                    await Task.Delay(10000);
                }
            });
        }

        private IEnumerable<IRole> GetPunishRoles()
        {
            if (!_cache.TryGetValue(CacheEnum.PunishedRoles, out IEnumerable<IRole> cacheEntry))
            {
                cacheEntry = Context.Guild.Roles.Where(x => x.Name.IndexOf(Configuration.Load(Context.Guild.Id).PunishedRole, StringComparison.OrdinalIgnoreCase) >= 0);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(CacheEnum.PunishedRoles, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        private async Task<IEnumerable<Punished>> GetAllPunishedAsync()
        {
            if (!_cache.TryGetValue(CacheEnum.PunishedUsers, out IEnumerable<Punished> cacheEntry))
            {
                cacheEntry = await _database.RetrieveAllPunishedAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(CacheEnum.PunishedUsers, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        private void ClearCache()
        {
            _cache.Remove(CacheEnum.PunishedUsers);
            _cache.Remove(CacheEnum.PunishedRoles);
        }

        private async Task<bool> CheckPunishedAsync(SocketGuildUser user, IEnumerable<Punished> allPunished)
        {
            var punishedRoles = GetPunishRoles();
            if (allPunished != null && punishedRoles != null)
            {
                var pun = allPunished.Where(x => x.UserId == (long)user.Id).SingleOrDefault();
                if (pun == null)
                    return false;

                var unpunishDate = pun.TimeOfPunishment.AddSeconds(pun.Duration);

                if (unpunishDate <= DateTime.UtcNow)
                {
                    await Unpunish(punishedRoles, user);
                    return false;
                }

                return true;
            }

            return false;
        }

        private async Task Unpunish(IEnumerable<IRole> roles, SocketGuildUser user)
        {
            foreach (var role in roles)
            {
                if (user.Roles.Contains(role))
                    await user.RemoveRoleAsync(role);
            }

            await _database.RemovePunishedAsync((long)user.Id);
            ClearCache();
        }

        private async void UnpunishTimed(IEnumerable<IRole> roles, SocketGuildUser user, int delay, CancellationTokenSource tokenSource)
        {
            await Task.Delay(delay, tokenSource.Token);
            await Unpunish(roles, user);
        }

        #endregion
    }
}
