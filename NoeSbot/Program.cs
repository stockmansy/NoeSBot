using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using Hangfire.MySql;
using Hangfire.Storage.SQLite;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NoeSbot.Activators;
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
        private const string _configurationPath = "config/noesbotconfig.json";
        private IConfigurationRoot _configurationRoot;
        private NBConfiguration.DataBaseConfig _dataBaseConfig;
        private string _token;

        static async Task Main(string[] args) => await new Program().Start();

        public async Task Start()
        {
            var serviceCollection = InitializeServiceCollection();
            LoadConfiguration();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            ConfigureLog4Net();

            _client.Log += Log;

            ConfigureServices(serviceCollection);
            
            var serviceProvider = serviceCollection
                    .AddLogging()
                    .AddMemoryCache()
                    .BuildServiceProvider();

            ConfigureHangfire(serviceProvider);

            using (var server = new BackgroundJobServer())
            {
                await InitializeConfigAndCommands(serviceProvider);

                await _client.LoginAsync(TokenType.Bot, _token);

                LogHelper.LogInfo("Starting the bot");
                await _client.StartAsync();

                LogHelper.LogInfo("The bot is running");

                await Task.Delay(-1);
            }
        }

        public Task Log(LogMessage message)
        {
            LogHelper.LogInfo(message.ToString());
            return Task.CompletedTask;
        }

        #region Private

        private IServiceCollection InitializeServiceCollection()
        {
            _configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile(_configurationPath, false, true)
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_configurationRoot);
            return serviceCollection;
        }

        private void LoadConfiguration()
        {
            _dataBaseConfig = _configurationRoot.GetSection(nameof(NBConfiguration.DataBaseConfig)).Get<NBConfiguration.DataBaseConfig>();
            _token = _configurationRoot.GetValue<string>($"{nameof(NBConfiguration.ExternalKeyTokens)}:{nameof(NBConfiguration.ExternalKeyTokens.DiscordToken)}");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
                        {
                            switch (_dataBaseConfig.UseDataBaseMode)
                            {
                                case NBConfiguration.DataBaseMode.MySQL:
                                    options.UseMySql(_dataBaseConfig.MySQLConnectionString);
                                    break;
                                case NBConfiguration.DataBaseMode.SQLite:
                                default:
                                    options.UseSqlite("Data Source=noesbot.db");
                                    break;
                            }
                        }, ServiceLifetime.Transient);
            services.AddSingleton(_client);
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<ModLogic>();
            services.AddSingleton<PunishLogic>();
            services.AddSingleton<NotifyLogic>();
            services.AddSingleton<EventLogic>();
            services.AddSingleton<CustomCommandLogic>();
            services.AddSingleton<MediaProcessor>();
            services.AddSingleton<MessageTriggers>();
            services.AddSingleton<GlobalConfig>();
            services.AddSingleton<UtilityModule>();
            services.AddSingleton<IPunishedService, PunishedService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IMessageTriggerService, MessageTriggerService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<INotifyService, NotifyService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<ICustomCommandService, CustomCommandService>();
            services.AddSingleton<ISerializedItemService, SerializedItemService>();
            services.AddSingleton<IActivityLogService, ActivityLogService>();
        }

        private void ConfigureHangfire(IServiceProvider serviceProvider)
        {
            switch (_dataBaseConfig.UseDataBaseMode)
            {
                case NBConfiguration.DataBaseMode.MySQL:
                    GlobalConfiguration.Configuration.UseStorage(new MySqlStorage(_dataBaseConfig.MySQLConnectionString, new MySqlStorageOptions()
                    {
                        TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                    }));
                    break;
                case NBConfiguration.DataBaseMode.SQLite:
                default:
                    GlobalConfiguration.Configuration.UseSQLiteStorage();
                    break;
            }

            GlobalConfiguration.Configuration.UseSerializerSettings(new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
        }

        private async Task InitializeConfigAndCommands(IServiceProvider serviceProvider)
        {
            var globConfig = serviceProvider.GetService<GlobalConfig>();
            var commandHandler = serviceProvider.GetService<CommandHandler>();

            await globConfig.LoadInGuildConfigs();
            await commandHandler.InstallCommands(serviceProvider);
        }

        private void ConfigureLog4Net()
        {
            var log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));

            var repo = LogManager.CreateRepository(
            Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            LogHelper.Initialize(typeof(Program));
        }

        #endregion
    }
}

