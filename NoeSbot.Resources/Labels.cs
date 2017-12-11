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
        public const ModuleEnum Audio_Id = ModuleEnum.Audio;

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

        #region CommonModule

        public const string Common_Module = "commonmodule_name";
        public const ModuleEnum Common_Id = ModuleEnum.Common;

        public const string Common_Say_Command = "say";
        public const string Common_Say_Alias_1 = "echo";
        public const string Common_Say_Name = "commonmodule_say_name";
        public const string Common_Say_Summary = "commonmodule_say_summary";

        public const string Common_SayTTS_Command = "saytts";
        public const string Common_SayTTS_Alias_1 = "echotts";
        public const string Common_SayTTS_Name = "commonmodule_saytts_name";
        public const string Common_SayTTS_Summary = "commonmodule_saytts_summary";

        public const string Common_Info_Command = "info";
        public const string Common_Info_Name = "commonmodule_info_name";
        public const string Common_Info_Summary = "commonmodule_info_summary";

        #endregion

        #region Configure Module

        public const string Configure_Module = "configuremodule_name";
        public const ModuleEnum Configure_Id = ModuleEnum.Configure;

        public const string Configure_GetConfig_Command = "getconfig";
        public const string Configure_GetConfig_Alias_1 = "getconfiguration";
        public const string Configure_GetConfig_Name = "configuremodule_getconfig_name";
        public const string Configure_GetConfig_Summary = "configuremodule_getconfig_summary";

        public const string Configure_SaveConfig_Command = "saveconfig";
        public const string Configure_SaveConfig_Name = "configuremodule_saveconfig_name";
        public const string Configure_SaveConfig_Summary = "configuremodule_saveconfig_summary";

        public const string Configure_LoadModule_Command = "loadmodule";
        public const string Configure_LoadModule_Alias_1 = "load";
        public const string Configure_LoadModule_Name = "configuremodule_loadmodule_name";
        public const string Configure_LoadModule_Summary = "configuremodule_loadmodule_summary";

        public const string Configure_LoadAllModules_Command = "loadallmodules";
        public const string Configure_LoadAllModules_Alias_1 = "loadall";
        public const string Configure_LoadAllModules_Name = "configuremodule_loadallmodules_name";
        public const string Configure_LoadAllModules_Summary = "configuremodule_loadallmodules_summary";

        public const string Configure_UnloadModule_Command = "unloadmodule";
        public const string Configure_UnloadModule_Alias_1 = "unload";
        public const string Configure_UnloadModule_Name = "configuremodule_unloadmodule_name";
        public const string Configure_UnloadModule_Summary = "configuremodule_unloadmodule_summary";

        #endregion

        #region Customize Module

        public const string Customize_Module = "customizemodule_name";
        public const ModuleEnum Customize_Id = ModuleEnum.Customize;

        public const string Customize_AddCustomPunish_Command = "addcustompunish";
        public const string Customize_AddCustomPunish_Name = "customizemodule_addcustompunish_name";
        public const string Customize_AddCustomPunish_Summary = "customizemodule_addcustompunish_summary";

        public const string Customize_AddCustom_Command = "addcustom";
        public const string Customize_AddCustom_Name = "customizemodule_addcustom_name";
        public const string Customize_AddCustom_Summary = "customizemodule_addcustom_summary";

        public const string Customize_GetCustomPunish_Command = "getcustompunish";
        public const string Customize_GetCustomPunish_Name = "customizemodule_getcustompunish_name";
        public const string Customize_GetCustomPunish_Summary = "customizemodule_getcustompunish_summary";

        public const string Customize_GetCustom_Command = "getcustom";
        public const string Customize_GetCustom_Name = "customizemodule_getcustom_name";
        public const string Customize_GetCustom_Summary = "customizemodule_getcustom_summary";

        public const string Customize_RemoveAllCustomPunish_Command = "removeallcustompunish";
        public const string Customize_RemoveAllCustomPunish_Name = "customizemodule_removeallcustompunish_name";
        public const string Customize_RemoveAllCustomPunish_Summary = "customizemodule_removeallcustompunish_summary";

        public const string Customize_RemoveAllCustom_Command = "removeallcustom";
        public const string Customize_RemoveAllCustom_Name = "customizemodule_removeallcustom_name";
        public const string Customize_RemoveAllCustom_Summary = "customizemodule_removeallcustom_summary";

        public const string Customize_RemoveCustomPunish_Command = "removecustompunish";
        public const string Customize_RemoveCustomPunish_Name = "customizemodule_removecustompunish_name";
        public const string Customize_RemoveCustomPunish_Summary = "customizemodule_removecustompunish_summary";

        public const string Customize_RemoveCustom_Command = "removecustom";
        public const string Customize_RemoveCustom_Name = "customizemodule_removecustom_name";
        public const string Customize_RemoveCustom_Summary = "customizemodule_removecustom_summary";

        #endregion

        #region Game Module

        public const string Game_Module = "gamemodule_name";
        public const ModuleEnum Game_Id = ModuleEnum.Game;

        public const string Game_FlipCoin_Command = "flipcoin";
        public const string Game_FlipCoin_Alias_1 = "flip";
        public const string Game_FlipCoin_Name = "gamemodule_flipcoin_name";
        public const string Game_FlipCoin_Summary = "gamemodule_flipcoin_summary";

        public const string Game_RockPaperScissors_Command = "rockpaperscissors";
        public const string Game_RockPaperScissors_Alias_1 = "rps";
        public const string Game_RockPaperScissors_Name = "gamemodule_rockpaperscissors_name";
        public const string Game_RockPaperScissors_Summary = "gamemodule_rockpaperscissors_summary";

        public const string Game_8Ball_Command = "8ball";
        public const string Game_8Ball_Alias_1 = "8b";
        public const string Game_8Ball_Name = "gamemodule_8ball_name";
        public const string Game_8Ball_Summary = "gamemodule_8ball_summary";

        public const string Game_Choose_Command = "choose";
        public const string Game_Choose_Alias_1 = "pick";
        public const string Game_Choose_Name = "gamemodule_choose_name";
        public const string Game_Choose_Summary = "gamemodule_choose_summary";

        public const string Game_Blame_Command = "blame";
        public const string Game_Blame_Alias_1 = "blamegame";
        public const string Game_Blame_Alias_2 = "randomlyblame";
        public const string Game_Blame_Name = "gamemodule_blame_name";
        public const string Game_Blame_Summary = "gamemodule_blame_summary";

        public const string Game_Roll_Command = "roll";
        public const string Game_Roll_Alias_1 = "d";
        public const string Game_Roll_Alias_2 = "dice";
        public const string Game_Roll_Alias_3 = "r";
        public const string Game_Roll_Name = "gamemodule_roll_name";
        public const string Game_Roll_Summary = "gamemodule_roll_summary";

        #endregion

        #region Help Module

        public const string Help_Module = "helpmodule_name";
        public const ModuleEnum Help_Id = ModuleEnum.Help;

        public const string Help_Marco_Command = "marco";
        public const string Help_Marco_Name = "helpmodule_marco_name";
        public const string Help_Marco_Summary = "helpmodule_marco_summary";

        public const string Help_Help_Command = "help";
        public const string Help_Help_Name = "helpmodule_help_name";
        public const string Help_Help_Summary = "helpmodule_help_summary";

        #endregion

        #region Media Module

        public const string Media_Module = "mediamodule_name";
        public const ModuleEnum Media_Id = ModuleEnum.Media;

        #endregion

        #region MessageTrigger Module

        public const string MessageTrigger_Module = "messagetriggermodule_name";
        public const ModuleEnum MessageTrigger_Id = ModuleEnum.MessageTrigger;

        public const string MessageTrigger_AddTrigger_Command = "addtrigger";
        public const string MessageTrigger_AddTrigger_Alias_1 = "trigger";
        public const string MessageTrigger_AddTrigger_Name = "messagetriggermodule_addtrigger_name";
        public const string MessageTrigger_AddTrigger_Summary = "messagetriggermodule_addtrigger_summary";

        public const string MessageTrigger_DeleteTrigger_Command = "deletetrigger";
        public const string MessageTrigger_DeleteTrigger_Alias_1 = "deltrigger";
        public const string MessageTrigger_DeleteTrigger_Alias_2 = "removetrigger";
        public const string MessageTrigger_DeleteTrigger_Name = "messagetriggermodule_deletetrigger_name";
        public const string MessageTrigger_DeleteTrigger_Summary = "messagetriggermodule_deletetrigger_summary";

        #endregion

        #region Mod Module

        public const string Mod_Module = "modmodule_name";
        public const ModuleEnum Mod_Id = ModuleEnum.Mod;

        public const string Mod_Nuke_Command = "nuke";
        public const string Mod_Nuke_Name = "modmodule_nuke_name";
        public const string Mod_Nuke_Summary = "modmodule_nuke_summary";

        #endregion

        #region Notify Module

        public const string Notify_Module = "notifymodule_name";
        public const ModuleEnum Notify_Id = ModuleEnum.Notify;

        public const string Notify_AddStream_Command = "addstream";
        public const string Notify_AddStream_Name = "notifymodule_addstream_name";
        public const string Notify_AddStream_Summary = "notifymodule_addstream_summary";

        public const string Notify_AddTwitchStream_Command = "addtwitchstream";
        public const string Notify_AddTwitchStream_Name = "notifymodule_addtwitchstream_name";
        public const string Notify_AddTwitchStream_Summary = "notifymodule_addtwitchstream_summary";

        public const string Notify_AddYoutubeStream_Command = "addyoutubestream";
        public const string Notify_AddYoutubeStream_Name = "notifymodule_addyoutubestream_name";
        public const string Notify_AddYoutubeStream_Summary = "notifymodule_addyoutubestream_summary";

        public const string Notify_AllStreams_Command = "allstreams";
        public const string Notify_AllStreams_Alias_1 = "getstreams";
        public const string Notify_AllStreams_Alias_2 = "streams";
        public const string Notify_AllStreams_Name = "notifymodule_allstreams_name";
        public const string Notify_AllStreams_Summary = "notifymodule_allstreams_summary";

        public const string Notify_RemoveStream_Command = "removestream";
        public const string Notify_RemoveStream_Alias_1 = "deletestream";
        public const string Notify_RemoveStream_Name = "notifymodule_removestream_name";
        public const string Notify_RemoveStream_Summary = "notifymodule_removestream_summary";

        public const string Notify_RemoveTwitchStream_Command = "removetwitchstream";
        public const string Notify_RemoveTwitchStream_Alias_1 = "deletetwitchstream";
        public const string Notify_RemoveTwitchStream_Name = "notifymodule_removetwitchstream_name";
        public const string Notify_RemoveTwitchStream_Summary = "notifymodule_removetwitchstream_summary";

        public const string Notify_RemoveYoutubeStream_Command = "removeyoutubestream";
        public const string Notify_RemoveYoutubeStream_Alias_1 = "deleteyoutubestream";
        public const string Notify_RemoveYoutubeStream_Name = "notifymodule_removeyoutubestream_name";
        public const string Notify_RemoveYoutubeStream_Summary = "notifymodule_removeyoutubestream_summary";

        #endregion

        #region Poll Module

        public const string Poll_Module = "pollmodule_name";
        public const ModuleEnum Poll_Id = ModuleEnum.Poll;

        public const string Poll_Poll_Command = "poll";
        public const string Poll_Poll_Alias_1 = "enquete";
        public const string Poll_Poll_Name = "pollmodule_poll_name";
        public const string Poll_Poll_Summary = "pollmodule_poll_summary";

        #endregion

        #region Profile Module

        public const string Profile_Module = "profilemodule_name";
        public const ModuleEnum Profile_Id = ModuleEnum.Profile;

        public const string Profile_Profile_Command = "profile";
        public const string Profile_Profile_Name = "profilemodule_profile_name";
        public const string Profile_Profile_Summary = "profilemodule_profile_summary";

        public const string Profile_ProfileItem_Command = "profileitem";
        public const string Profile_ProfileItem_Alias_1 = "addprofileitem";
        public const string Profile_ProfileItem_Alias_2 = "addprofile";
        public const string Profile_ProfileItem_Name = "profilemodule_profileitem_name";
        public const string Profile_ProfileItem_Summary = "profilemodule_profileitem_summary";

        public const string Profile_RemoveProfileItem_Command = "removeprofileitem";
        public const string Profile_RemoveProfileItem_Alias_1 = "deleteprofileitem";
        public const string Profile_RemoveProfileItem_Name = "profilemodule_removeprofileitem_name";
        public const string Profile_RemoveProfileItem_Summary = "profilemodule_removeprofileitem_summary";

        #endregion

        #region Punish Module

        public const string Punish_Module = "punishmodule_name";
        public const ModuleEnum Punish_Id = ModuleEnum.Punish;

        public const string Punish_Punish_Command = "punish";
        public const string Punish_Punish_Alias_1 = "silence";
        public const string Punish_Punish_Name = "punishmodule_punish_name";
        public const string Punish_Punish_Summary = "punishmodule_punish_summary";

        public const string Punish_Punished_Command = "punished";
        public const string Punish_Punished_Alias_1 = "silenced";
        public const string Punish_Punished_Name = "punishmodule_punished_name";
        public const string Punish_Punished_Summary = "punishmodule_punished_summary";

        public const string Punish_Unpunish_Command = "unpunish";
        public const string Punish_Unpunish_Alias_1 = "unsilence";
        public const string Punish_Unpunish_Name = "punishmodule_unpunish_name";
        public const string Punish_Unpunish_Summary = "punishmodule_unpunish_summary";

        #endregion

        #region Urban Module

        public const string Urban_Module = "urbanmodule_name";
        public const ModuleEnum Urban_Id = ModuleEnum.Urban;

        public const string Urban_Urban_Command = "urban";
        public const string Urban_Urban_Alias_1 = "lookup";
        public const string Urban_Urban_Name = "urbanmodule_urban_name";
        public const string Urban_Urban_Summary = "urbanmodule_urban_summary";

        #endregion

        #region Utility Module

        public const string Utility_Module = "utilitymodule_name";
        public const ModuleEnum Utility_Id = ModuleEnum.Utility;

        public const string Utility_UserInfo_Command = "userinfo";
        public const string Utility_UserInfo_Name = "utilitymodule_userinfo_name";
        public const string Utility_UserInfo_Summary = "utilitymodule_userinfo_summary";

        public const string Utility_RandomMember_Command = "randommember";
        public const string Utility_RandomMember_Alias_1 = "rndm";
        public const string Utility_RandomMember_Alias_2 = "rndmbr";
        public const string Utility_RandomMember_Name = "utilitymodule_randommember_name";
        public const string Utility_RandomMember_Summary = "utilitymodule_randommember_summary";

        #endregion

        #region Event Module

        public const string Event_Module = "eventmodule_name";
        public const ModuleEnum Event_Id = ModuleEnum.Event;

        public const string Event_StartEvent_Command = "startevent";
        public const string Event_StartEvent_Alias_1 = "addevent";
        public const string Event_StartEvent_Name = "eventmodule_startevent_name";
        public const string Event_StartEvent_Summary = "eventmodule_startevent_summary";

        public const string Event_UpdateEvent_Command = "updateevent";
        public const string Event_UpdateEvent_Name = "eventmodule_updateevent_name";
        public const string Event_UpdateEvent_Summary = "eventmodule_updateevent_summary";

        public const string Event_StopEvent_Command = "stopevent";
        public const string Event_StopEvent_Name = "eventmodule_stopevent_name";
        public const string Event_StopEvent_Summary = "eventmodule_stopevent_summary";

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
                var modId = modProps.Where(x => x.Name.EndsWith("_id", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                var mod = new ModuleInfoModel()
                {
                    Name = GetText(modName?.GetValue("").ToString()) ?? "",
                    Id = (ModuleEnum)modId?.GetValue(ModuleEnum.Help),
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
                        Command = propName,
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
            var result = new CommandInfoModel()
            {
                Name = "Not found",
                Description = "",
                Fields = new List<FieldInfoModel>(),
                Examples = new List<string>()
            };

            var command = AllFields.Where(x => x.FieldType == typeof(string) && x.GetValue("").ToString().Equals(commandname, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (command != null)
            {
                var split = command.Name.Split('_');
                var cmd = split[1];
                var commandPrefix = $"{split[0]}_{cmd}";
                var commandName = AllFields.Where(x => x.Name.EndsWith($"{commandPrefix}_Name", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.GetValue("").ToString() ?? "";
                var commandSummary = AllFields.Where(x => x.Name.EndsWith($"{commandPrefix}_Summary", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.GetValue("").ToString() ?? "";
                var examples = AllFields.Where(x => x.Name.ToLowerInvariant().Contains($"{commandPrefix.ToLowerInvariant()}_example")).Select(x => GetText(x.GetValue("").ToString())).ToList() ?? new List<string>();
                var aliases = new List<string>();

                foreach (var alias in AllFields.Where(x => x.Name.ToLowerInvariant().Contains(commandPrefix.ToLowerInvariant()) && x.Name.ToLowerInvariant().Contains("alias")).ToList())
                {
                    aliases.Add(alias.GetValue("").ToString());
                }

                result.Name = GetText(commandName);
                result.Command = cmd;
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

            if (string.IsNullOrWhiteSpace(summary))
                return model;

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

                    fields = fields.Replace($"{{field}}{field}{{/field}}", "");

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

                    examples = examples.Replace($"{{example}}{example}{{/example}}", "");

                    model.Examples.Add(example);

                    maxExamples++;
                }
            }

            model.Description = descr;

            return model;
        }

        private static string GetSubstring(string full, string between)
        {
            if (string.IsNullOrWhiteSpace(full))
                return "";

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