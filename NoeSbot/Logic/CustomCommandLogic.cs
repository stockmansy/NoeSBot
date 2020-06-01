using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;
using Discord.WebSocket;
using NoeSbot.Helpers;
using NoeSbot.Database.Services;
using Microsoft.Extensions.Caching.Memory;
using NoeSbot.Enums;
using NoeSbot.Models;
using Newtonsoft.Json;
using NoeSbot.Resources;

namespace NoeSbot.Logic
{
    public class CustomCommandLogic
    {
        private readonly ICustomCommandService _customCommandService;
        private IMemoryCache _cache;

        public CustomCommandLogic(ICustomCommandService customCommandService, IMemoryCache memoryCache)
        {
            _customCommandService = customCommandService;
            _cache = memoryCache;
        }

        public async Task<bool> Process(ICommandContext context, IEnumerable<CommandInfo> commands, string customCommandName, string customCommandValue, IServiceProvider service)
        {
            //TODO don't have the database model here etc...
            if (!_cache.TryGetValue(CacheEnum.CustomCommmands, out IEnumerable<CustomCommand> customCommands))
            {
                var allCommands = await _customCommandService.RetrieveAllCustomCommandsAsync((long)context.Guild.Id);
                var result = new List<CustomCommand>();

                foreach (var command in allCommands)
                {
                    switch (command.Type)
                    {
                        case Database.Models.CustomCommand.CustomCommandType.Punish:
                            var punishInfo = JsonConvert.DeserializeObject<Database.Models.CustomPunishCommand>(command.Value);
                            result.Add(new CustomCommand
                            {
                                GuildId = context.Guild.Id,
                                Name = command.Name,
                                PunishCommand = new CustomCommand.CustomPunishCommand
                                {
                                    UserId = (ulong)punishInfo.UserId,
                                    DurationInSec = punishInfo.Duration,
                                    Reason = punishInfo.Reason
                                }
                            });
                            break;
                        case Database.Models.CustomCommand.CustomCommandType.Unpunish:
                            var unpunishInfo = JsonConvert.DeserializeObject<Database.Models.CustomUnpunishCommand>(command.Value);
                            result.Add(new CustomCommand
                            {
                                GuildId = context.Guild.Id,
                                Name = command.Name,
                                UnPunishCommand = new CustomCommand.CustomUnPunishCommand
                                {
                                    UserId = (ulong)unpunishInfo.UserId
                                }
                            });
                            break;
                        case Database.Models.CustomCommand.CustomCommandType.Alias:
                            var aliasInfo = JsonConvert.DeserializeObject<Database.Models.CustomAliasCommand>(command.Value);
                            result.Add(new CustomCommand
                            {
                                GuildId = context.Guild.Id,
                                Name = command.Name,
                                AliasCommand = new CustomCommand.CustomAliasCommand
                                {
                                    AliasCommand = aliasInfo.AliasCommand,
                                    RemoveMessages = aliasInfo.RemoveResult
                                }
                            });
                            break;
                    }
                }

                customCommands = result;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(CacheEnum.CustomCommmands, customCommands, cacheEntryOptions);
            }

            var loadedModules = GlobalConfig.GetGuildConfig(context.Guild.Id).LoadedModules;

            foreach (var custom in customCommands)
            {
                if (customCommandName.Equals(custom.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var parms = new List<object>();
                    IResult res = null;

                    // Punish section
                    if (!loadedModules.Contains((int)ModuleEnum.Punish))
                        continue;

                    if (custom.PunishCommand != null)
                    {
                        var command = commands.Where(com => com.Name == Labels.Punish_Punish_Command && com.Parameters.Count == 3).FirstOrDefault();

                        var preResult = await command.CheckPreconditionsAsync(context);
                        if (!preResult.IsSuccess)
                            throw new Exception("Not allowed to execute the command");

                        var user = await context.Guild.GetUserAsync(custom.PunishCommand.UserId);
                        parms.Add((SocketGuildUser)user);
                        parms.Add($"{custom.PunishCommand.DurationInSec.ToString()}s");
                        parms.Add(custom.PunishCommand.Reason);
                        res = await command.ExecuteAsync(context, parms, command.Parameters, service);
                        return res.IsSuccess;
                    }

                    if (custom.UnPunishCommand != null)
                    {
                        var command = commands.Where(com => com.Name == Labels.Punish_Unpunish_Command && com.Parameters.Count == 1 && com.Parameters.Where(p => p.Name == "user").Any()).FirstOrDefault();

                        var preResult = await command.CheckPreconditionsAsync(context);
                        if (!preResult.IsSuccess)
                            throw new Exception("Not allowed to execute the command");

                        var user = await context.Guild.GetUserAsync(custom.UnPunishCommand.UserId);
                        parms.Add((SocketGuildUser)user);
                        res = await command.ExecuteAsync(context, parms, command.Parameters, service);
                        return res.IsSuccess;
                    }

                    if (custom.AliasCommand != null)
                    {
                        await context.Channel.SendMessageAsync($"{custom.AliasCommand.AliasCommand} {customCommandValue}");
                        try
                        {
                            if (custom.AliasCommand.RemoveMessages)
                                await context.Message.DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogWarning($"Message was already removed: {ex}");
                        }
                        
                        return true;
                    }
                }
            }

            return false;
        }       
    }
}
