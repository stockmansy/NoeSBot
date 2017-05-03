using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Attributes;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Helpers;
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

        #region Help text

        [Command("help")]
        [Summary("Get info about a specific command")]
        public async Task HelpCommandAsync()
        {
            var builder = new StringBuilder();
            builder.AppendLine("```");
            builder.AppendLine("1 required parameter: commandname");
            builder.AppendLine("Provide info about a certain command");
            builder.AppendLine("```");
            await ReplyAsync(builder.ToString());
        }

        #endregion

        #region Commands

        [Command("getconfig")]
        [Alias("getconfiguration")]
        [Summary("Retrieve the configuration for this server")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task GetConfigsAsync()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var user = Context.User as SocketGuildUser;

                var config = Configuration.Load(Context.Guild.Id);
                var builder = new EmbedBuilder() {
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
                    x.Value = config.Prefix;
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
        }

        [Command("saveconfig")]
        [Summary("Save a configuration item")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task SaveConfigAsync([Summary("Name of the configuration")] string configName,
                                          [Summary("The user")] SocketGuildUser user)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
        }        

        [Command("saveconfig")]
        [Summary("Save a configuration item")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task SaveConfigAsync([Summary("Name of the configuration")] string configName,
                                          [Summary("Configuration value")] string value)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var success = true;
                switch (configName.ToLowerInvariant())
                {
                    case "prefix":
                        success = await _service.SaveConfigurationItem(((long)Context.Guild.Id), (int)ConfigurationEnum.Prefix, value);
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
        }

        [Command("loadmodule")]
        [Alias("load")]
        [Summary("Add a loaded module")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task LoadModuleAsync([Summary("Name of the module")] string moduleName)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
            {
                var success = true;
                var modules = CommonHelper.GetModuleEnums();
                foreach (var module in modules)
                {
                    var moduleId = (int)module;
                    var isInt = int.TryParse(moduleName, out int parsedId);
                    if (module.ToString().Equals(moduleName, StringComparison.OrdinalIgnoreCase) ||
                        (isInt && parsedId == moduleId)) { 
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
        }

        [Command("loadallmodules")]
        [Alias("loadall")]
        [Summary("Add a loaded module")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task LoadAllModuleAsync()
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
        }

        [Command("unloadmodule")]
        [Alias("unload")]
        [Summary("Add a loaded module")]
        [MinPermissions(AccessLevel.ServerOwner)]
        public async Task UnLoadModuleAsync([Summary("Name of the module")] string moduleName)
        {
            if (!Context.Message.Author.IsBot && !Context.Message.Author.IsWebhook)
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
        }

        #endregion
    }
}
