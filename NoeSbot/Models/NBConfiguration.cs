using Newtonsoft.Json;
using NoeSbot.Attributes;
using NoeSbot.Enums;
using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NoeSbot
{
    public class NBConfiguration
    {
        public class DataBaseConfig
        {
            public string MySQLConnectionString { get; set; }
            public DataBaseMode UseDataBaseMode { get; set; } = DataBaseMode.SQLite;
        }

        public class ExternalKeyTokens
        {
            public string DiscordToken { get; set; }
            public string TwitchClientId { get; set; }
            public string YoutubeApiKey { get; set; }
        }

        public class DefaultConfig : ICloneable
        {
            [Configuration(ConfigurationEnum.Owners)]
            public ulong[] Owners { get; set; }

            [DefaultValue(new char[] { '!' })]
            [Configuration(ConfigurationEnum.Prefixes)]
            public char[] Prefixes { get; set; }

            [DefaultValue("silenced")]
            [Configuration(ConfigurationEnum.PunishedRole)]
            public string PunishedRole { get; set; }

            [DefaultValue("media_room")]
            [Configuration(ConfigurationEnum.MediaChannel)]
            public string MediaChannel { get; set; }

            [DefaultValue("general")]
            [Configuration(ConfigurationEnum.GeneralChannel)]
            public string GeneralChannel { get; set; }

            [DefaultValue("initiate")]
            [Configuration(ConfigurationEnum.NewUserRole)]
            public string NewUserRole { get; set; }

            [DefaultValue(5)]
            [Configuration(ConfigurationEnum.AudioVolume)]
            public int AudioVolume { get; set; } = 5;

            [Configuration(ConfigurationEnum.LoadedModules)]
            public int[] LoadedModules
            {
                get
                {
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
                set
                {
                    _loadedModules = value;
                }
            }

            #region Private members

            [JsonIgnore]
            private int[] _loadedModules;

            [JsonIgnore]
            private char[] _prefixes;

            #endregion

            #region Clone 

            public object Clone() => MemberwiseClone();

            public DefaultConfig GetClone() => (DefaultConfig)Clone();

            #endregion
        }

        public enum DataBaseMode
        {
            SQLite,
            MySQL
        }
    }
}