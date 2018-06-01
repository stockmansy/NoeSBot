using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NoeSbot.Database;
using NoeSbot.Database.Services;
using NoeSbot.Handlers;
using NoeSbot.Helpers;
using NoeSbot.Logic;
using NoeSbot.Modules;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace NoeSbot
{
    public class Program
    {
        private DiscordSocketClient _client;
        
        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();
        
        public async Task Start()
        {
            Configuration.EnsureExists();
            
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            var log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));

            var repo = log4net.LogManager.CreateRepository(
            Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
            
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

            LogHelper.LogWithConsole("Starting the bot");
            await _client.StartAsync();

            LogHelper.LogWithConsole("The bot is running");

            await Task.Delay(-1);
        }

        public Task Log(LogMessage message)
        {
            LogHelper.LogWithConsole(message.ToString());
            return Task.CompletedTask;
        }

        private IServiceCollection ConfigureServices()
        {
            return new ServiceCollection()
                        .AddDbContext<DatabaseContext>(options => options.UseMySql(Configuration.Load().ConnectionString))
                        .AddSingleton(_client)
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<ModLogic>()
                        .AddSingleton<PunishLogic>()
                        .AddSingleton<NotifyLogic>()
                        .AddSingleton<EventLogic>()
                        .AddSingleton<CustomCommandLogic>()
                        .AddSingleton<MediaProcessor>()
                        .AddSingleton<MessageTriggers>()
                        .AddSingleton<IPunishedService, PunishedService>()
                        .AddSingleton<IConfigurationService, ConfigurationService>()
                        .AddSingleton<IMessageTriggerService, MessageTriggerService>()
                        .AddSingleton<IProfileService, ProfileService>()
                        .AddSingleton<INotifyService, NotifyService>()
                        .AddSingleton<IHttpService, HttpService>()
                        .AddSingleton<IEventService, EventService>()
                        .AddSingleton<ICustomCommandService, CustomCommandService>();
        }
    }
}

