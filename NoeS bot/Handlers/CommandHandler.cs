using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace NoeSbot.Handlers
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IDependencyMap _map;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(CommandService commands, DiscordSocketClient client, IDependencyMap map, ILoggerFactory loggerFactory)
        {
            _commands = commands;
            _client = client;
            _map = map;
            _logger = loggerFactory.CreateLogger<CommandHandler>();
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommand;

            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            var guild = (message.Channel as Discord.IGuildChannel)?.Guild;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix(Configuration.Load(guild.Id).Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new CommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            var result = await _commands.ExecuteAsync(context, argPos, _map);

            if (!result.IsSuccess)
            {
                #if DEBUG
                await context.Channel.SendMessageAsync(result.ErrorReason);
                #endif
                _logger.LogError(result.ErrorReason);
            }
        }
    }
}
