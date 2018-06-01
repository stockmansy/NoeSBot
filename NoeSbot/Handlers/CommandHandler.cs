﻿using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using Discord;
using NoeSbot.Enums;
using NoeSbot.Logic;
using NoeSbot.Database.Services;
using System.Threading;
using NoeSbot.Resources;
using Microsoft.Extensions.Caching.Memory;

namespace NoeSbot.Handlers
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _provider;
        private readonly ILogger<CommandHandler> _logger;
        private IMessageTriggerService _msgTrgSrvs;
        private ModLogic _modLogic;
        private PunishLogic _punishLogic;
        private NotifyLogic _notifyLogic;
        private EventLogic _eventLogic;
        private bool _notifyTaskRunning;
        private CancellationTokenSource _tokenSource;
        private CustomCommandLogic _customCommandLogic;
        private MediaProcessor _mediaProcessor;
        private MessageTriggers _messageTriggers;

        public CommandHandler(CommandService commands, DiscordSocketClient client, ILoggerFactory loggerFactory, IMessageTriggerService msgTrgSrvs, MediaProcessor mediaProcessor, MessageTriggers messageTriggers, ModLogic modLogic, PunishLogic punishLogic, NotifyLogic notifyLogic, EventLogic eventLogic, CustomCommandLogic customCommandLogic)
        {
            _commands = commands;
            _client = client;
            _logger = loggerFactory.CreateLogger<CommandHandler>();
            _msgTrgSrvs = msgTrgSrvs;
            _modLogic = modLogic;
            _punishLogic = punishLogic;
            _notifyLogic = notifyLogic;
            _eventLogic = eventLogic;
            _customCommandLogic = customCommandLogic;
            _mediaProcessor = mediaProcessor;
            _messageTriggers = messageTriggers;
            _tokenSource = new CancellationTokenSource();
        }

        public async Task InstallCommands(IServiceProvider provider)
        {
            _provider = provider;

            // Hook the event handlers
            _client.MessageReceived += MessageReceivedHandler;
            _client.MessageUpdated += MessageUpdatedHandler;
            _client.UserJoined += _modLogic.UserJoined;
            _client.Ready += Ready;

            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public Task Ready()
        {
            if (!_notifyTaskRunning)
            {
                // TODO Maybe change this...
                var run = Task.Run(async () => await _notifyLogic.Run(_tokenSource.Token));
                _notifyTaskRunning = true;
            }

            _client.ReactionAdded -= _eventLogic.OnReactionAdded;
            _client.ReactionAdded += _eventLogic.OnReactionAdded;

            return Task.CompletedTask;
        }

        public async Task MessageReceivedHandler(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            var guild = (message.Channel as IGuildChannel)?.Guild;

            if (guild == null)
                return;

            // Create a Command Context
            var context = new CommandContext(_client, message);

            // Handle normal messages            
            if (!messageParam.Author.IsBot && !messageParam.Author.IsWebhook)
            {
                var cont = await ProcessMessage(context);
                if (!cont) // Should it continue?
                    return;
            }

            // If it isn't a command return
            if (!(message.HasCharPrefix(Configuration.Load(guild.Id).Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

            // Execute the command. (result does not indicate a return value, rather an object stating if the command executed succesfully)
            var result = await _commands.ExecuteAsync(context, argPos, _provider, MultiMatchHandling.Best);

            if (!result.IsSuccess)
            {
                // Execute custom commmands
                try
                {
                    var success = await _customCommandLogic.Process(context, _commands.Commands, context.Message.Content.Substring(argPos).Trim(), _provider);
                    if (success)
                        return;
                }
                catch (Exception ex)
                {
                    await context.Channel.SendMessageAsync(ex.Message);
                    return;
                }
#if DEBUG
                await context.Channel.SendMessageAsync(result.ErrorReason);
#endif
                _logger.LogError(result.ErrorReason);
            }
        }

        private async Task<bool> ProcessMessage(CommandContext context)
        {
            try
            {
                var loadedModules = Configuration.Load(context.Guild.Id).LoadedModules;

                if (loadedModules.Contains((int)ModuleEnum.Mod))
                {
                    if (!(await _modLogic.Process(context)))
                        return false;
                }

                if (loadedModules.Contains((int)ModuleEnum.Punish))
                {
                    await _punishLogic.HandleMessage(context);
                }

                if (loadedModules.Contains((int)ModuleEnum.Media))
                {
                    await _mediaProcessor.Process(context);
                }

                if (loadedModules.Contains((int)ModuleEnum.MessageTrigger))
                {
                    await _messageTriggers.Process(context);
                }

                return true;
            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(ex.Message);
                return false;
            }
        }

        private async Task MessageUpdatedHandler(Cacheable<IMessage, ulong> cachedmessage, SocketMessage messageParam, ISocketMessageChannel channel)
        {
            try
            {
                if (!messageParam.Author.IsBot && !messageParam.Author.IsWebhook)
                {
                    var message = messageParam as SocketUserMessage;
                    if (message == null) return;

                    var hasCachedEmbeds = cachedmessage.HasValue && cachedmessage.Value.Embeds.Any();
                    var hasEmbeds = message.Embeds.Any();

                    if (!hasCachedEmbeds && hasEmbeds)
                    {
                        var context = new CommandContext(_client, message);

                        var loadedModules = Configuration.Load(context.Guild.Id).LoadedModules;
                        if (loadedModules.Contains((int)ModuleEnum.Media))
                        {
                            await _mediaProcessor.Process(context);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await messageParam.Channel.SendMessageAsync(ex.Message);
            }
        }
    }
}
