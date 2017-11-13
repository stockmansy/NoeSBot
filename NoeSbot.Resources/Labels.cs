using Newtonsoft.Json.Linq;
using NoeSbot.Resources.Models;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Linq;

namespace NoeSbot.Resources
{
    public static class Labels
    {
        private static IList<string> _modules = null;
        private static IList<FieldInfo> _allFields = null;

        #region Labels

        #region Modules

        public static IList<string> Modules
        {
            get
            {
                if (_modules == null)
                    SetModules();
                return _modules;
            }
        }

        public static IList<FieldInfo> AllFields
        {
            get
            {
                if (_allFields == null)
                    SetAllFields();
                return _allFields;
            }
        }

        #endregion

        #region AudioModule

        public const string Audio_Module = "audiomodule_name";

        public const string Audio_AudioInfo_Command = "audioinfo";
        public const string Audio_AudioInfo_Alias_1 = "musicinfo";
        public const string Audio_AudioInfo_Name = "audiomodule_audioinfo_name";
        public const string Audio_AudioInfo_Summary = "audiomodule_audioinfo_summary";

        public const string Audio_Volume_Command = "volume";
        public const string Audio_Volume_Alias_1 = "v";
        public const string Audio_Volume_Name = "audiomodule_volume_name";
        public const string Audio_Volume_Summary = "audiomodule_volume_summary";

        public const string Audio_Play_Command = "play";
        public const string Audio_Play_Alias_1 = "p";
        public const string Audio_Play_Alias_2 = "playaudio";
        public const string Audio_Play_Alias_3 = "playsong";
        public const string Audio_Play_Name = "audiomodule_play_name";
        public const string Audio_Play_Summary = "audiomodule_play_summary";

        public const string Audio_Stop_Command = "stop";
        public const string Audio_Stop_Alias_1 = "s";
        public const string Audio_Stop_Alias_2 = "stopaudio";
        public const string Audio_Stop_Alias_3 = "stopsong";
        public const string Audio_Stop_Name = "audiomodule_stop_name";
        public const string Audio_Stop_Summary = "audiomodule_stop_summary";

        public const string Audio_Skip_Command = "skip";
        public const string Audio_Skip_Name = "audiomodule_skip_name";
        public const string Audio_Skip_Summary = "audiomodule_skip_summary";

        public const string Audio_Current_Command = "current";
        public const string Audio_Current_Alias_1 = "currentaudio";
        public const string Audio_Current_Alias_2 = "currentsong";
        public const string Audio_Current_Name = "audiomodule_current_name";
        public const string Audio_Current_Summary = "audiomodule_current_summary";


        #endregion

        #endregion

        public static string GetText(string id)
        {
            return Resource.ResourceManager.GetString(id);
        }

        public static IList<ModuleInfoModel> GetModules()
        {
            var result = new List<ModuleInfoModel>();
            
            foreach (var module in Modules)
            {
                var modProps = AllFields.Where(x => x.Name.StartsWith(module, System.StringComparison.OrdinalIgnoreCase));

                var modName = modProps.Where(x => x.Name.EndsWith("module", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                var mod = new ModuleInfoModel()
                {
                    Name = GetText(modName?.GetValue("").ToString()) ?? "",
                    Commands = new List<CommandInfoModel>()
                };

                foreach (var command in modProps.Where(x => x.Name.EndsWith("command", System.StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    var propName = command.Name.Split('_')[1];
                    var aliases = new List<string>();

                    foreach (var alias in modProps.Where(x => x.Name.ToLowerInvariant().Contains(propName.ToLowerInvariant()) && x.Name.ToLowerInvariant().Contains("alias")).ToList())
                    {
                        aliases.Add(alias.GetValue("").ToString());
                    }

                    var commandName = modProps.Where(x => x.Name.EndsWith($"{propName}_Name", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.GetValue("").ToString() ?? "";
                    var commandSummary = modProps.Where(x => x.Name.EndsWith($"{propName}_Summary", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.GetValue("").ToString() ?? "";
                    var examples = modProps.Where(x => x.Name.ToLowerInvariant().Contains($"{propName.ToLowerInvariant()}_example")).Select(x => GetText(x.GetValue("").ToString())).ToList() ?? new List<string>();

                    var commandInfoModel = new CommandInfoModel
                    {
                        Name = GetText(commandName),
                        Description = "",
                        Alias = aliases,
                        Fields = new List<FieldInfoModel>(),
                        Examples = new List<string>()
                    };

                    commandInfoModel = ProcessDescription(commandInfoModel, GetText(commandSummary));

                    mod.Commands.Add(commandInfoModel);
                }

                result.Add(mod);
            }

            return result;
        }

        public static CommandInfoModel GetCommandInfo(string commandname)
        {
            var result = new CommandInfoModel() {
                Name = "Not found",
                Description = "",
                Fields = new List<FieldInfoModel>(),
                Examples = new List<string>()
            };

            var command = AllFields.Where(x => x.GetValue("").ToString().Equals(commandname, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (command != null)
            {
                var split = command.Name.Split('_');
                var commandPrefix = $"{split[0]}_{split[1]}";
                var commandName = AllFields.Where(x => x.Name.EndsWith($"{commandPrefix}_Name", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.GetValue("").ToString() ?? "";
                var commandSummary = AllFields.Where(x => x.Name.EndsWith($"{commandPrefix}_Summary", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.GetValue("").ToString() ?? "";
                var examples = AllFields.Where(x => x.Name.ToLowerInvariant().Contains($"{commandPrefix.ToLowerInvariant()}_example")).Select(x => GetText(x.GetValue("").ToString())).ToList() ?? new List<string>();
                var aliases = new List<string>();

                foreach (var alias in AllFields.Where(x => x.Name.ToLowerInvariant().Contains(commandPrefix.ToLowerInvariant()) && x.Name.ToLowerInvariant().Contains("alias")).ToList())
                {
                    aliases.Add(alias.GetValue("").ToString());
                }

                result.Name = GetText(commandName);
                result.Alias = aliases;

                result = ProcessDescription(result, GetText(commandSummary));
            }

            return result;
        }

        #region Private

        private static void SetModules()
        {
            _modules = AllFields.Where(x => x.Name.Contains("_")).Select(x => x.Name.Split('_')[0]?.ToLowerInvariant()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        }

        private static void SetAllFields()
        {
            var type = typeof(Labels);
            _allFields = type.GetRuntimeFields().ToList();
        }

        private static CommandInfoModel ProcessDescription(CommandInfoModel model, string summary)
        {
            var fields = GetSubstring(summary, "fields");
            var descr = summary;

            var indexOf = summary.IndexOf("{fields");
            if (indexOf <= 0)
                indexOf = summary.IndexOf("{examples");

            if (indexOf > 0)
                descr = descr.Substring(0, indexOf);

            if (!string.IsNullOrWhiteSpace(fields))
            {
                var maxFields = 0;
                while (fields.Length > 0 && maxFields < 100)
                {
                    var field = GetSubstring(fields, "field");
                    if (string.IsNullOrWhiteSpace(field))
                        break;

                    fields = fields.Replace(field, "");

                    var name = GetSubstring(field, "name");
                    var value = GetSubstring(field, "value");

                    model.Fields.Add(new FieldInfoModel
                    {
                        Name = name,
                        Value = value
                    });

                    maxFields++;
                }
            }

            var examples = GetSubstring(summary, "examples");
            if (!string.IsNullOrWhiteSpace(examples))
            {
                var maxExamples = 0;
                while (examples.Length > 0 && maxExamples < 100)
                {
                    var example = GetSubstring(examples, "example");
                    if (string.IsNullOrWhiteSpace(example))
                        break;

                    examples = examples.Replace(example, "");
                    
                    model.Examples.Add(example);

                    maxExamples++;
                }
            }

            model.Description = descr;

            return model;
        }

        private static string GetSubstring(string full, string between)
        {
            var startString = $"{{{between}}}";
            var endString = $"{{/{between}}}";
            var startIndex = full.IndexOf(startString);
            var endIndex = full.IndexOf(endString);

            if (startIndex <= 0 || endIndex <= 0)
                return "";

            var otherIndex = startIndex + startString.Length;
            return full.Substring(otherIndex, endIndex - otherIndex);
        }

        #endregion
    }
}