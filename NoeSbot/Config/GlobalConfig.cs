﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NoeSbot.Attributes;
using NoeSbot.Database;
using NoeSbot.Database.Services;
using NoeSbot.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NoeSbot
{
    public class GlobalConfig
    {
        private readonly IConfigurationService _configurationService;
        private readonly NBConfiguration.DefaultConfig _defaultConfig;
        private readonly ILogger<GlobalConfig> _logger;

        private static Dictionary<ulong, NBConfiguration.DefaultConfig> _guildSpecificConfig;

        public NBConfiguration.DefaultConfig DefaultConfig { get; set; }

        public GlobalConfig(IConfigurationRoot config, IConfigurationService configurationService, ILoggerFactory loggerFactory)
        {
            _configurationService = configurationService;
            _guildSpecificConfig = new Dictionary<ulong, NBConfiguration.DefaultConfig>();
            _logger = loggerFactory.CreateLogger<GlobalConfig>();

            _defaultConfig = config.GetSection(nameof(NBConfiguration.DefaultConfig)).Get<NBConfiguration.DefaultConfig>(c => c.BindNonPublicProperties = false);
            SetDefaultValuesIfNull(); // Had to be added because ^ .Get doesn't override the default values, instead it adds (so the default value would always be added to the config files values)
        }

        public async Task LoadInGuildConfigs()
        {
            var configs = await _configurationService.RetrieveAllConfigurationsAsync();

            if (configs.NotNullOrEmpty())
            {
                var guildConfigs = configs.ToLookup(c => c.GuildId);

                foreach (var config in guildConfigs)
                {
                    _guildSpecificConfig.Add((ulong)config.Key, MapGuildValuesOnDefaultValues(config));
                }
            }
        }

        public static NBConfiguration.DefaultConfig GetGuildConfig(ulong guildId)
        {
            if (_guildSpecificConfig == null || !_guildSpecificConfig.TryGetValue(guildId, out var result))
                return new NBConfiguration.DefaultConfig();

            return result;
        }

        #region Lets hide this reflection stuff *whistles*

        private void SetDefaultValuesIfNull()
        {
            foreach (var prop in typeof(NBConfiguration.DefaultConfig).GetProperties())
            {
                var attr = prop.GetCustomAttribute<DefaultValueAttribute>();
                var value = prop.GetValue(_defaultConfig);
                if (attr != null && value == null)
                {
                    var isEnumerable = typeof(Enumerable).IsAssignableFrom(prop.PropertyType);
                    if (!isEnumerable ||
                        (isEnumerable && !((IEnumerable<object>)value).Any()))
                        prop.SetValue(_defaultConfig, attr.Value);
                }
            }
        }

        private NBConfiguration.DefaultConfig MapGuildValuesOnDefaultValues(IGrouping<long, Database.Models.Config> configs)
        {
            var result = _defaultConfig.GetClone();

            foreach (var prop in typeof(NBConfiguration.DefaultConfig).GetProperties())
            {
                // Get the configuration attribute of the property to fetch the guild specific values from the database
                var configAttr = prop.GetCustomAttribute<ConfigurationAttribute>();                
                if (configAttr != null)
                {
                    var enumV = configAttr.GetConfigurationEnum();

                    var configValues = configs.Where(c => c.ConfigurationTypeId == (int)enumV).ToList();

                    // If there are any guild specific values, override the default values if the values can be properly parsed
                    if (configValues.Any())
                    {
                        try
                        {
                            if (prop.PropertyType.IsArray)
                            {
                                var converter = TypeDescriptor.GetConverter(prop.PropertyType.GetElementType());
                                var objs = new object[configValues.Count];
                                for (var i = 0; i < configValues.Count; i++)
                                    objs[i] = converter.ConvertFrom(configValues[i].Value);

                                var destinationArray = Array.CreateInstance(prop.PropertyType.GetElementType(), objs.Length);
                                Array.Copy(objs, destinationArray, objs.Length);

                                result.GetType().GetProperty(prop.Name).SetValue(result, destinationArray);
                            }
                            else
                            {
                                var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                                var convResult = converter.ConvertFrom(configValues.Last().Value);

                                result.GetType().GetProperty(prop.Name).SetValue(result, convResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Invalid configuration file entry: {ex.Message}");
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}