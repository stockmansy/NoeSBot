using Newtonsoft.Json;
using NoeSbot.Database.Services;
using NoeSbot.Enums;
using NoeSbot.Helpers;
using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot
{
    public class Configuration
    {
        private static Configuration _config;
        private static Dictionary<ulong, Configuration> _guildSpecificConfig;

        [JsonIgnore]
        public static string FileName { get; private set; } = "config/configuration.json";
        public ulong[] Owners { get; set; }
        public char Prefix { get; set; } = '!';
        public string Token { get; set; } = "";
        public string PunishedRole { get; set; } = "silenced";
        public string MediaChannel { get; set; } = "media_room";
        public string GeneralChannel { get; set; } = "general";
        public string NewUserRole { get; set; } = "initiate";
        public string TwitchClientId { get; set; }
        public string YoutubeApiKey { get; set; }
        public int AudioVolume { get; set; } = 5;

        private int[] _loadedModules;
        public int[] LoadedModules {
            get {

                if (_loadedModules == null)
                {
                    var loadedModules = Enum.GetValues(typeof(ModuleEnum));
                    var allLoadedModules = new List<int>();
                    foreach (ModuleEnum lModule in loadedModules)
                    {
                        allLoadedModules.Add((int)lModule);
                    }
                    _loadedModules = allLoadedModules.ToArray();
                }                

                return _loadedModules;
            }
            set {
                _loadedModules = value;
            }
        }
        public string ConnectionString { get; set; }

        public static void EnsureExists()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))
            {
                string path = Path.GetDirectoryName(file);
                Directory.CreateDirectory(path);

                var config = Configuration.LoadEmbedded();

                Console.WriteLine("Please enter your token: ");
                string token = Console.ReadLine();

                Console.WriteLine("Please enter your database host (localhost):");
                string dbHost = Console.ReadLine();

                Console.WriteLine("Please enter your database post (3306):");
                string dbPort = Console.ReadLine();

                Console.WriteLine("Please enter your database name (noesbot):");
                string dbName = Console.ReadLine();

                Console.WriteLine("Please enter your database user (noesbot):");
                string dbUser = Console.ReadLine();

                Console.WriteLine("Please enter your database password (123456):");
                string dbPassword = Console.ReadLine();

                config.Token = token;
                config.ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};", dbHost, dbPort, dbName, dbUser, dbPassword);
                config.SaveJson();
            }
            Console.WriteLine("Configuration Loaded");
        }

        public void SaveJson()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, ToJson());
        }

        public static Configuration LoadEmbedded()
        {
            var assembly = typeof(Configuration).GetTypeInfo().Assembly;
            Stream resource = assembly.GetManifestResourceStream("NoeSbot.Config.configuration.example.json");
            using (StreamReader reader = new StreamReader(resource))
            {
                string result = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Configuration>(result);
            }
        }

        public static Configuration Load()
        {
            if (_config == null)
            {
                string file = Path.Combine(AppContext.BaseDirectory, FileName);
                _config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
            }
            return _config;
        }

        public static Configuration Load(ulong guildId)
        {
            if (_guildSpecificConfig == null || !_guildSpecificConfig.TryGetValue(guildId, out Configuration result))
                return Load();

            return result;
        }
        
        public static async Task LoadAsync(IConfigurationService service)
        {
            var configs = await service.RetrieveAllConfigurationsAsync();
            if (configs != null && configs.Count > 0)
            {
                _guildSpecificConfig = new Dictionary<ulong, Configuration>();
                
                var guildIds = configs.Select(x => x.GuildId).Distinct();

                foreach(var guildId in guildIds)
                {
                    var exConfig = CloneJson(Load());

                    var guildConfigs = configs.Where(x => x.GuildId == guildId).Distinct();

                    var owners = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.Owners).Select(x => ulong.Parse(x.Value)).ToArray();
                    if (owners.Length > 0)
                        exConfig.Owners = owners;

                    var prefix = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.Prefix).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(prefix))
                        exConfig.Prefix = prefix.First();

                    var punishedRole = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.PunishedRole).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(punishedRole))
                        exConfig.PunishedRole = punishedRole;

                    var mediaChannel = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.MediaChannel).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(mediaChannel))
                        exConfig.MediaChannel = mediaChannel;

                    var generalChannel = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.GeneralChannel).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(generalChannel))
                        exConfig.GeneralChannel = generalChannel;

                    var loadedModules = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.LoadedModules).Select(x => int.Parse(x.Value)).ToArray();
                    if (loadedModules.Length > 0)
                        exConfig.LoadedModules = loadedModules;

                    var audioVolume = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.AudioVolume).Select(x => int.Parse(x.Value)).FirstOrDefault();
                    if (audioVolume > 0)
                        exConfig.AudioVolume = audioVolume;

                    var newUserRole = guildConfigs.Where(x => x.ConfigurationTypeId == (int)ConfigurationEnum.NewUserRole).Select(x => x.Value).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(newUserRole))
                        exConfig.NewUserRole = newUserRole;


                    _guildSpecificConfig.Add((ulong)guildId, exConfig);
                }
            }
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static Configuration CloneJson(Configuration source)
        {
            if (Object.ReferenceEquals(source, null))
                return default(Configuration);
            
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            return JsonConvert.DeserializeObject<Configuration>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }
}
