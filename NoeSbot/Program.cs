using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NoeSbot.Database;
using NoeSbot.Database.Services;
using NoeSbot.Handlers;
using NoeSbot.Modules;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NoeSbot
{
    public class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private DependencyMap _map;

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();
        
        public async Task Start()
        {
            Configuration.EnsureExists();            
            Globals.LoadGlobals();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            _client.Log += Log;
            
            _commands = new CommandService();
                        
            _map = new DependencyMap();
            _map.Add(_client);
            _map.Add(serviceProvider.GetService<IMemoryCache>());
            _map.Add(serviceProvider.GetService<IPunishedService>());
            _map.Add(serviceProvider.GetService<IConfigurationService>());
            _map.Add(serviceProvider.GetService<IMessageTriggerService>());
            _map.Add(serviceProvider.GetService<IProfileService>());

            await Configuration.LoadAsync(serviceProvider.GetService<IConfigurationService>());

            var commandHandler = new CommandHandler(_commands, _client, _map, serviceProvider.GetService<ILoggerFactory>());
            await commandHandler.InstallCommands();

            var messageHandler = new MessageHandler(_commands, _client, _map, serviceProvider.GetService<ILoggerFactory>());
            await messageHandler.InstallHandlers();

            await _client.LoginAsync(TokenType.Bot, Configuration.Load().Token);

            

            Console.WriteLine("Starting the bot");
            await _client.StartAsync();

            Console.WriteLine("The bot is running");
            await Task.Delay(-1);
        }

        public Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<DatabaseContext>(options =>
                    options.UseMySql(Configuration.Load().ConnectionString));
            serviceCollection.AddSingleton<IPunishedService, PunishedService>();
            serviceCollection.AddSingleton<IConfigurationService, ConfigurationService>();
            serviceCollection.AddSingleton<IMessageTriggerService, MessageTriggerService>();
            serviceCollection.AddSingleton<IProfileService, ProfileService>();
        }
    }
}

