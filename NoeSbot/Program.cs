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
        private DiscordSocketClient _client;

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();
        
        public async Task Start()
        {
            Configuration.EnsureExists();            
            Globals.LoadGlobals();
            
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            _client.Log += Log;

            var serviceCollection = ConfigureServices();

            var serviceProvider = serviceCollection
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider();

            await Configuration.LoadAsync(serviceProvider.GetService<IConfigurationService>());

            // Init Commands
            await serviceProvider.GetService<CommandHandler>().InstallCommands(serviceProvider);

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

        private IServiceCollection ConfigureServices()
        {
            return new ServiceCollection()
                        .AddDbContext<DatabaseContext>(options => options.UseMySql(Configuration.Load().ConnectionString))
                        .AddSingleton(_client)
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<IPunishedService, PunishedService>()
                        .AddSingleton<IConfigurationService, ConfigurationService>()
                        .AddSingleton<IMessageTriggerService, MessageTriggerService>()
                        .AddSingleton<IProfileService, ProfileService>();
        }
    }
}

