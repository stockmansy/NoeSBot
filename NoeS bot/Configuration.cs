using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoeSbot
{
    public class Configuration
    {
        private static Configuration _config;

        [JsonIgnore]
        public static string FileName { get; private set; } = "config/configuration.json";
        public ulong[] Owners { get; set; }
        public char Prefix { get; set; } = '!';
        public string Token { get; set; } = "";
        public string PunishedRole { get; set; } = "silenced";
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
                _config =  JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
            }
            return _config;
        }

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
