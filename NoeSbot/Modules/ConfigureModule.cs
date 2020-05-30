using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using NoeSbot.Attributes;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NoeSbot.Modules
{
    [Name("ignore")]
    [ModuleName(ModuleEnum.Configure)]
    public class ConfigureModule : ModuleBase
    {
        private readonly IConfigurationService _service;
        private readonly GlobalConfig _globalConfig;

        public ConfigureModule(IConfigurationService service, GlobalConfig globalConfig)
        {
            _service = service;
            _globalConfig = globalConfig;
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

            var config = GlobalConfig.GetGuildConfig(Context.Guild.Id);
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
                x.Value = string.Join(',', config.Prefixes);
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
                loadedString += $"{lModule}{Environment.NewLine}";

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
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Configure_SaveConfig_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
        }

        [Command(Labels.Configure_SaveConfig_Command)]
        [MinPermissions(AccessLevel.ServerOwner)]
        [BotAccess(BotAccessAttribute.AccessLevel.BotsRefused)]
        public async Task SaveConfigAsync([Summary("Name of the configuration")] string configName,
                                          [Summary("The user")] SocketGuildUser user)
        {
            var success = true;

            var targetedConfigs = GlobalConfig.GetTargetedConfigs();

            var targetedConfig = targetedConfigs.EmptyIfNull().FirstOrDefault(tc => tc.Name.Equals(configName, StringComparison.OrdinalIgnoreCase) || tc.Name.Singularize().Equals(configName, StringComparison.OrdinalIgnoreCase));
            if (targetedConfig != default((string, ConfigurationAttribute)))
                success = await _service.AddConfigurationItem(((long)Context.Guild.Id), (int)targetedConfig.ConfigAttr.GetConfigurationEnum(), user.Id.ToString());
            else
            {
                await ReplyAsync("Configuration name not found");
                return;
            }

            await _globalConfig.LoadInGuildConfigs();

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

            var nonTargetedConfigs = GlobalConfig.GetNonTargetedConfigs();

            var nonTargetedConfig = nonTargetedConfigs.EmptyIfNull().FirstOrDefault(tc => tc.Name.Equals(configName, StringComparison.OrdinalIgnoreCase) || tc.Name.Singularize().Equals(configName, StringComparison.OrdinalIgnoreCase));
            if (nonTargetedConfig != default((string, ConfigurationAttribute)))
                success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)nonTargetedConfig.ConfigAttr.GetConfigurationEnum(), value);
            else
            {
                await ReplyAsync("Configuration name not found");
                return;
            }

            await _globalConfig.LoadInGuildConfigs();

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
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Configure_LoadModule_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
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

            await _globalConfig.LoadInGuildConfigs();

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

            await _globalConfig.LoadInGuildConfigs();

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
            await ReplyAsync("", false, CommonDiscordHelper.GetHelp(Labels.Configure_UnloadModule_Command, GlobalConfig.GetGuildConfig(Context.Guild.Id).Prefixes, user.GetColor()));
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

            await _globalConfig.LoadInGuildConfigs();

            if (success)
                await ReplyAsync("Saved the configuration");
            else
                await ReplyAsync("Failed to save the configuration");
        }

        #endregion

        #endregion
    }
}
