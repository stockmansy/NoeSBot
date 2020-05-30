using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot.Modules
{
    [ModuleName(ModuleEnum.Help)]
    public class HelpModule : ModuleBase
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        #region Commands

        #region Marco

        [Command(Labels.Help_Marco_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsAllowed)]
        public async Task MarcoPoloAsync()
        {
            await ReplyAsync("Polo");
        }

        #endregion

        #region Help

        [Command(Labels.Help_Help_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task HelpAsync()
        {
            await Context.Message.DeleteAsync();
            var user = Context.User as SocketGuildUser;

            var prefixes = GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes;
            var loadedModules = GlobalConfig.GetGuildConfig(Context.Guild.Id).LoadedModules;

            await user.SendMessageAsync("", false, CommonDiscordHelper.GetModules(prefixes, user.GetColor(), loadedModules));
        }

        [Command(Labels.Help_Help_Command)]
        [MinPermissions(AccessLevel.User)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsAllowed)]
        public async Task HelpAsync(string command)
        {
            var user = Context.User as SocketGuildUser;
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, the {command} command could not be found.");
                return;
            }

            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        #endregion

        #endregion
    }
}
