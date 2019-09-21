using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using NoeSbot.Database.Services;
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
    [Name("ignore")]
    [ModuleName(ModuleEnum.Configure)]
    public class ConfigureModule : ModuleBase
    {
        private IConfigurationService _service;

        public ConfigureModule(IConfigurationService service)
        {
            _service = service;
        }

        #region Commands

        #region Get Config

        [Command(Labels.Configure_GetConfig_Command)]
        [Alias(Labels.Configure_GetConfig_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task GetConfigsAsync()
        {
            var user = Context.User as SocketGuildUser;

            var config = Configuration.Load(Context.Guild.Id);
            var builder = new EmbedBuilder()
            {
                Color = user.GetColor(),
                Description = "You have the following configuration:"
            };

            var owners = "";
            for (var i = 0; i < config.Owners.Length; i++)
            {
                var owner = await Context.Guild.GetUserAsync(config.Owners[i]);
                owners += $"{owner.Username}{Environment.NewLine}";
            }

            builder.AddField(x =>
            {
                x.Name = "Owners";
                x.Value = owners;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Prefix";
                x.Value = config.Prefixes;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Punished Role";
                x.Value = config.PunishedRole;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Media Channel";
                x.Value = config.MediaChannel;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "General Channel";
                x.Value = config.GeneralChannel;
                x.IsInline = false;
            });

            var loadedModules = CommonHelper.GetModuleEnums(config.LoadedModules);
            var loadedString = "";
            foreach (var lModule in loadedModules)
                loadedString += $"{lModule.ToString()}{Environment.NewLine}";

            builder.AddField(x =>
            {
                x.Name = "Loaded Modules";
                x.Value = loadedString;
                x.IsInline = false;
            });

            await ReplyAsync("", false, builder.Build());
        }

        #endregion

        #region Save Config

        [Command(Labels.Configure_SaveConfig_Command)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SaveConfigAsync()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Configure_SaveConfig_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Configure_SaveConfig_Command)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SaveConfigAsync([Summary("Name of the configuration")] string configName,
                                          [Summary("The user")] SocketGuildUser user)
        {
            var success = true;
            switch (configName.ToLowerInvariant())
            {
                case "owner":
                case "owners":
                    success = await _service.AddConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.Owners, user.Id.ToString());
                    break;
            }

            await Configuration.LoadAsync(_service);

            if (success)
                await ReplyAsync("Saved the configuration");
            else
                await ReplyAsync("Failed to save the configuration");
        }

        [Command(Labels.Configure_SaveConfig_Command)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SaveConfigAsync([Summary("Name of the configuration")] string configName,
                                          [Summary("Configuration value")] string value)
        {
            var success = true;
            switch (configName.ToLowerInvariant())
            {
                case "prefix":
                    success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.Prefixes, value);
                    break;
                case "punishedrole":
                    success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.PunishedRole, value);
                    break;
                case "mediachannel":
                    success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.MediaChannel, value);
                    break;
                case "generalchannel":
                    success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.GeneralChannel, value);
                    break;
            }

            await Configuration.LoadAsync(_service);

            if (success)
                await ReplyAsync("Saved the configuration");
            else
                await ReplyAsync("Failed to save the configuration");
        }

        #endregion

        #region Load Module

        [Command(Labels.Configure_LoadModule_Command)]
        [Alias(Labels.Configure_LoadModule_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task LoadModuleAsync()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Configure_LoadModule_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Configure_LoadModule_Command)]
        [Alias(Labels.Configure_LoadModule_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task LoadModuleAsync([Summary("Name of the module")] string moduleName)
        {
            var success = true;
            var modules = CommonHelper.GetModuleEnums();
            foreach (var module in modules)
            {
                var moduleId = (int)module;
                var isInt = int.TryParse(moduleName, out int parsedId);
                if (module.ToString().Equals(moduleName, StringComparison.OrdinalIgnoreCase) ||
                    (isInt && parsedId == moduleId))
                {
                    success = await _service.AddConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.LoadedModules, moduleId.ToString());
                    break;
                }
            }

            await Configuration.LoadAsync(_service);

            if (success)
                await ReplyAsync("Saved the configuration");
            else
                await ReplyAsync("Failed to save the configuration");
        }

        #endregion

        #region Load All Modules

        [Command(Labels.Configure_LoadAllModules_Command)]
        [Alias(Labels.Configure_LoadAllModules_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task LoadAllModuleAsync()
        {
            var success = true;
            var modules = CommonHelper.GetModuleEnums();
            foreach (var module in modules)
            {
                var moduleId = (int)module;
                success = await _service.AddConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.LoadedModules, moduleId.ToString());
            }

            await Configuration.LoadAsync(_service);

            if (success)
                await ReplyAsync("Saved the configuration");
            else
                await ReplyAsync("Failed to save the configuration");
        }

        #endregion

        #region Unload Module

        [Command(Labels.Configure_UnloadModule_Command)]
        [Alias(Labels.Configure_UnloadModule_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UnLoadModuleAsync()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync("", false, CommonHelper.GetHelp(Labels.Configure_UnloadModule_Command, Configuration.Load(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Configure_UnloadModule_Command)]
        [Alias(Labels.Configure_UnloadModule_Alias_1)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task UnLoadModuleAsync([Summary("Name of the module")] string moduleName)
        {
            var success = true;
            var modules = CommonHelper.GetModuleEnums();
            foreach (var module in modules)
            {
                var moduleId = (int)module;
                var isInt = int.TryParse(moduleName, out int parsedId);
                if (module.ToString().Equals(moduleName, StringComparison.OrdinalIgnoreCase) ||
                    (isInt && parsedId == moduleId))
                {
                    success = await _service.RemoveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.LoadedModules, moduleId.ToString());
                    break;
                }
            }

            await Configuration.LoadAsync(_service);

            if (success)
                await ReplyAsync("Saved the configuration");
            else
                await ReplyAsync("Failed to save the configuration");
        }

        #endregion

        #endregion
    }
}
