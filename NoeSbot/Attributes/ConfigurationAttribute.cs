using Discord.Commands;
using Discord.WebSocket;
using NoeSbot.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NoeSbot.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationAttribute : Attribute
    {
        private readonly ConfigurationEnum _configEnum;

        public ConfigurationAttribute(ConfigurationEnum configEnum)
        {
            _configEnum = configEnum;
        }

        public ConfigurationEnum GetConfigurationEnum()
        {
            return _configEnum;
        }
    }
}
