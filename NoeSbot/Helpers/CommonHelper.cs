using Discord;
using Discord.WebSocket;
using NoeSbot.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class CommonHelper
    {
        public static int GetTimeInSeconds(string input)
        {
            var seconds = 0;

            int minutes = 0;
            bool isNumeric = int.TryParse(input, out minutes);

            if (isNumeric)
            {
                seconds = minutes * 60;
            }
            else
            {
                var digits = from c in input
                             where Char.IsDigit(c)
                             select c;

                var alphas = from c in input
                             where !Char.IsDigit(c)
                             select c;

                var time = int.Parse(string.Join("", digits));
                var timestring = string.Join("", alphas).Replace(" ", "");

                switch (timestring)
                {
                    case "s":
                    case "sec":
                    case "seconds":
                        seconds = time;
                        break;
                    case "m":
                    case "min":
                    case "minutes":
                    default:
                        seconds = time * 60;
                        break;
                    case "h":
                    case "hours":
                        seconds = time * 60 * 60;
                        break;
                    case "d":
                    case "day":
                    case "days":
                        seconds = time * 60 * 60 * 24;
                        break;
                }
            }

            return seconds;
        }

        public static string GetTimeString(int seconds)
        {
            var timespan = TimeSpan.FromSeconds(seconds);
            return timespan.ToString(@"hh\:mm\:ss");
        }

        public static string GetTimeString(DateTime timeof, int seconds)
        {
            var timeWhenUnpunished = timeof.AddSeconds(seconds);
            var difference = timeWhenUnpunished.Subtract(DateTime.UtcNow);

            return ToReadableString(difference);
        }

        public static string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static Color GetColor(this SocketGuildUser user)
        {
            return user.Roles.Last().Color;
        }

        public static string GetAliases(this IReadOnlyList<string> aliases)
        {
            var result = aliases.First();
            for (var i = 1; i < aliases.Count(); i++)
                result += $" ({aliases[i]})";
            return result;
        }

        public static string GetProcessedString(this string input)
        {
            return input.Replace("{br}", Environment.NewLine).Trim();
        }

        public static IEnumerable<ModuleEnum> GetModuleEnums(int[] ids = null)
        {
            if (ids == null || ids.Length <= 0)
                return Enum.GetValues(typeof(ModuleEnum)).Cast<ModuleEnum>();

            var result = new List<ModuleEnum>();
            for (var i = 0; i < ids.Length; i++)
            {
                result.Add((ModuleEnum)Enum.Parse(typeof(ModuleEnum), ids[i].ToString()));
            }

            return result;
        }

        public static FileInfo[] GetImagesFromDirectory(string directory)
        {
            var d = new DirectoryInfo(directory);
            return d.GetFiles();
        }

        public static string[] GetSplitIntoPages(string input, int maxChars = 1000)
        {
            var charCount = 0;
            var lines = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.GroupBy(w => (charCount += (((charCount % maxChars) + w.Length + 1 >= maxChars)
                            ? maxChars - (charCount % maxChars) : 0) + w.Length + 1) / maxChars)
                        .Select(g => string.Join(" ", g.ToArray()))
                        .ToArray();


            //var nPages = (int)Math.Ceiling((double)input.Length / 1000);
            //var result = new string[nPages];

            //if (input.Length <= maxChars)
            //{                
            //    result[0] = input;
            //    return result;
            //}

            //for (var i = 0; i < nPages; i++) {
            //    var sub = input.Substring(i * maxChars);
            //    if (sub.Length > maxChars)
            //        sub = input.Substring(i * maxChars, maxChars);
            //    result[i] = sub;
            //}

            //return result;
        }
    }
}
