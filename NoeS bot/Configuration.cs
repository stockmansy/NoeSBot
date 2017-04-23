using Newtonsoft.Json;
using NoeSbot.Enums;
using NoeSbot.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
        public string ConnectionString { get; set; }

        public static void EnsureExists()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))
            {
                string path = Path.GetDirectoryName(file);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var config = new Configuration();

                Console.WriteLine("Please enter your token: ");
                string token = Console.ReadLine();

                config.Token = token;
                config.SaveJson();
            }
            Console.WriteLine("Configuration Loaded");
        }

        public void SaveJson()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, ToJson());
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

        // Very rough version
        public static async Task LoadAsync(IConfigurationService service)
        {
            var configs = await service.RetrieveAllConfigurationsAsync();
            if (configs != null && configs.Count > 0)
            {
                var exConfig = Load();
                foreach (var config in configs)
                {
                    //switch (config.ConfigurationTypeId)
                    //{
                    //    case (int)ConfigurationEnum.Owners:
                    //        config.Value.Split(';');
                    //}

                    _guildSpecificConfig.Add((ulong)config.GuildId, exConfig);
                }
            }
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
