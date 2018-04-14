using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Database.Models;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoeSbot.Logic
{
    public class PunishLogic
    {
        private readonly IPunishedService _database;
        private readonly DiscordSocketClient _client;
        private IMemoryCache _cache;
        private bool _runPunishedCheck = false;

        public PunishLogic(IPunishedService database, DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _database = database;
            _client = client;
            _cache = memoryCache;
        }

        public async Task HandleMessage(ICommandContext context)
        {
            var allPunished = await GetAllPunishedAsync();
            if (allPunished.Where(x => x.UserId == (long)context.Message.Author.Id).Any())
            {
                var shouldDelete = await CheckPunishedAsync(context, (SocketGuildUser)context.Message.Author, allPunished);
                if (shouldDelete)
                    await context.Message.DeleteAsync();
            }
        }

        public async Task<CustomPunishItem> GetCustomPunish(SocketGuildUser user)
        {
            var result = new CustomPunishItem
            {
                HasCustom = false
            };

            var customMsgs = await _database.RetrieveAllCustomPunishedAsync((long)user.Id);
            if (customMsgs != null && customMsgs.Count() > 0)
            {
                var rnd = new Random();
                int r = rnd.Next(customMsgs.Count());
                var randomCustomMsg = customMsgs[r];
                if (randomCustomMsg == null)
                    throw new Exception("Random custom message is missing");

                result.HasCustom = true;

                if (!string.IsNullOrWhiteSpace(randomCustomMsg.DelayMessage))
                    result.DelayMessage = randomCustomMsg.DelayMessage.GetProcessedString();

                result.ReasonMessage = randomCustomMsg.Reason.GetProcessedString();
            }

            return result;
        }

        public async Task Punish(ICommandContext context, SocketGuildUser user, string time, string reason, int durationInSecs)
        {
            var success = await _database.SavePunishedAsync((long)user.Id, DateTime.UtcNow, durationInSecs, reason);
            var punishedRoles = GetPunishRoles(context);
            if (success && punishedRoles != null)
            {
                LogHelper.Log($"Punish user {user.Username} ({user.Id}) for guild {context.Guild.Name}", LogLevel.Debug);

                foreach (var role in punishedRoles)
                    await user.AddRoleAsync(role);

                ClearCache();

                if (!_runPunishedCheck)
                    StartPunishedCheck(context);
            }
            else
                throw new Exception("Could not successfully punish the user");
        }

        public async Task<bool?> UnPunish(ICommandContext context, SocketGuildUser user)
        {
            var all = await GetAllPunishedAsync();
            if (all.Where(x => x.UserId == (long)context.User.Id).Any())
                return null;

            var success = await _database.RemovePunishedAsync((long)user.Id);
            var punishedRoles = GetPunishRoles(context);

            if (success && punishedRoles != null)
            {
                LogHelper.Log($"Unpunish user {user.Username} ({user.Id}) for guild {context.Guild.Name}", LogLevel.Debug);

                foreach (var role in punishedRoles)
                    await user.RemoveRoleAsync(role);

                ClearCache();

                await StopPunishedCheckAsync(context);

                return true;
            }

            return false;
        }

        public async Task<bool?> UnPunishAll(ICommandContext context)
        {
            var all = await GetAllPunishedAsync();
            if (all.Where(x => x.UserId == (long)context.User.Id).Any())
                return null;

            var success = await _database.RemovePunishedAsync();
            var punishedRoles = GetPunishRoles(context);

            if (success && punishedRoles != null)
            {
                LogHelper.Log($"Unpunish all", LogLevel.Debug);

                foreach (var pun in all)
                {
                    var iuser =await context.Guild.GetUserAsync((ulong)pun.UserId);
                    var user = (SocketGuildUser)iuser;

                    foreach (var role in punishedRoles)
                    {
                        await user.RemoveRoleAsync(role);
                    }
                }

                ClearCache();

                await StopPunishedCheckAsync(context);

                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Punished>> GetPunished(ICommandContext context)
        {
            var allPunished = await GetAllPunishedAsync();
            var result = new List<Punished>();
            var punishedRoles = GetPunishRoles(context);

            foreach (var pun in allPunished)
            {
                var user = await context.Guild.GetUserAsync((ulong)pun.UserId);
                var punishTime = CommonHelper.GetTimeString(pun.TimeOfPunishment, pun.Duration);
                if (punishTime.StartsWith("-"))
                {
                    await Unpunish(punishedRoles, (SocketGuildUser)user);
                    await StopPunishedCheckAsync(context);
                }
                else
                {
                    result.Add(pun);
                }
            }

            return result;
        }

        private void StartPunishedCheck(ICommandContext context)
        {
            _runPunishedCheck = true;
            DoPunishedCheckTask(context);
        }

        private async Task StopPunishedCheckAsync(ICommandContext context)
        {
            var allPunished = await GetAllPunishedAsync();
            if (allPunished.Count() <= 0)
            {
                LogHelper.Log($"Stop punished check", LogLevel.Debug);
                _runPunishedCheck = false;
            }
        }

        private void DoPunishedCheckTask(ICommandContext context)
        {
            LogHelper.Log($"Starting punish check", LogLevel.Debug);

            Task.Run(async () =>
            {
                while (true && _runPunishedCheck)
                {
                    var allPunished = await GetAllPunishedAsync();
                    var punishedRoles = GetPunishRoles(context);
                    var count = allPunished.Count();

                    if (count <= 0)
                    {
                        LogHelper.Log($"Stop punish check", LogLevel.Debug);
                        _runPunishedCheck = false;
                        return;
                    }

                    foreach (var pun in allPunished)
                    {
                        var user = await context.Guild.GetUserAsync((ulong)pun.UserId);
                        var isStillPunished = await CheckPunishedAsync(context, (SocketGuildUser)user, allPunished);

                        if (!isStillPunished && count == 1)
                        {
                            LogHelper.Log($"Stop punish check after checkpunished", LogLevel.Debug);
                            _runPunishedCheck = false;
                        }
                    }

                    await Task.Delay(10000);
                }
            });
        }

        private IEnumerable<IRole> GetPunishRoles(ICommandContext context)
        {
            if (!_cache.TryGetValue(CacheEnum.PunishedRoles, out IEnumerable<IRole> cacheEntry))
            {
                cacheEntry = context.Guild.Roles.Where(x => x.Name.IndexOf(Configuration.Load(context.Guild.Id).PunishedRole, StringComparison.OrdinalIgnoreCase) >= 0);

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

        private async Task<bool> CheckPunishedAsync(ICommandContext context, SocketGuildUser user, IEnumerable<Punished> allPunished)
        {
            var punishedRoles = GetPunishRoles(context);
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
            LogHelper.Log($"Unpunish user {user.Username} ({user.Id})", LogLevel.Debug);

            foreach (var role in roles)
            {
                if (user.Roles.Contains(role)) { 
                    await user.RemoveRoleAsync(role);
                    LogHelper.Log($"Attempted to remove role {role.Name}", LogLevel.Debug);
                }
            }

            await _database.RemovePunishedAsync((long)user.Id);
            ClearCache();
        }

        private async void UnpunishTimed(IEnumerable<IRole> roles, SocketGuildUser user, int delay, CancellationTokenSource tokenSource)
        {
            await Task.Delay(delay, tokenSource.Token);
            await Unpunish(roles, user);
        }
    }
}
