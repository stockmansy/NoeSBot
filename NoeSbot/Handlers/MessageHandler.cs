using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using NoeSbot.Modules;
using Discord;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using NoeSbot.Logic;
using NoeSbot.Enums;

namespace NoeSbot.Handlers
{
    public class MessageHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IDependencyMap _map;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(CommandService commands, DiscordSocketClient client, IDependencyMap map, ILoggerFactory loggerFactory)
        {
            _commands = commands;
            _client = client;
            _map = map;
            _logger = loggerFactory.CreateLogger<MessageHandler>();
        }

        public async Task InstallHandlers()
        {
            _client.MessageUpdated += MessageUpdatedHandler;
            _client.MessageReceived += MessageReceivedHandler;
            await Task.CompletedTask;
        }

        private async Task MessageReceivedHandler(SocketMessage messageParam)
        {
            try
            {
                if (!messageParam.Author.IsBot && !messageParam.Author.IsWebhook)
                {
                    var message = messageParam as SocketUserMessage;
                    if (message == null) return;

                    var context = new CommandContext(_client, message);

                    var loadedModules = Configuration.Load(context.Guild.Id).LoadedModules;
                    if (loadedModules.Contains((int)ModuleEnum.Media))
                    {
                        var mediaProcessor = new MediaProcessor(context, _map);
                        await mediaProcessor.Process();
                    }
                }
            }
            catch (Exception ex)
            {
                await messageParam.Channel.SendMessageAsync(ex.Message);
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
                            var mediaProcessor = new MediaProcessor(context, _map);
                            await mediaProcessor.Process();
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
