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
using NoeSbot.Resources;

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

        #region Add Trigger

        [Command(Labels.MessageTrigger_AddTrigger_Command)]
        [Alias(Labels.MessageTrigger_AddTrigger_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddTrigger()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.MessageTrigger_AddTrigger_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.MessageTrigger_AddTrigger_Command)]
        [Alias(Labels.MessageTrigger_AddTrigger_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task AddTrigger([Summary("The trigger")] string trig,
                                        [Summary("The message triggered")] string mess,
                                        [Summary("Optional tts")] bool tts = false)
        {
            var success = await _database.SaveMessageTrigger(trig.ToLower(), mess, tts, (long)Context.Guild.Id);
            if (success)
                await ReplyAsync("Trigger for " + trig.ToLower() + " successfully added");
            else
                await ReplyAsync("Something went wrong. Trigger not saved.");

            RemoveCache();
        }

        #endregion

        #region Delete Trigger

        [Command(Labels.MessageTrigger_DeleteTrigger_Command)]
        [Alias(Labels.MessageTrigger_DeleteTrigger_Alias_1, Labels.MessageTrigger_DeleteTrigger_Alias_2)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task DeleteTrigger()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.MessageTrigger_DeleteTrigger_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.MessageTrigger_DeleteTrigger_Command)]
        [Alias(Labels.MessageTrigger_DeleteTrigger_Alias_1, Labels.MessageTrigger_DeleteTrigger_Alias_2)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task DeleteTrigger([Summary("The trigger")] string trig)
        {
            var success = await _database.DeleteMessageTrigger(trig.ToLower(), (long)Context.Guild.Id);
            if (success)
                await ReplyAsync("Trigger for " + trig.ToLower() + " successfully removed");
            else
                await ReplyAsync("Trigger does not exist or something else went wrong.");

            RemoveCache();
        }

        #endregion

        #region Private

        private void RemoveCache()
        {
            _cache.Remove(CacheEnum.MessageTriggers);
        }

        #endregion
    }
}
