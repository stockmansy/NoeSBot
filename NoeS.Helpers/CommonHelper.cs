using NoeSbot.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class CommonHelper
    {
        private static Random _random = new Random();

        public static int GetTimeInSeconds(string input, int defaultInSeconds = 300)
        {
            var seconds = defaultInSeconds;

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

                if (!int.TryParse(string.Join("", digits), out int time))
                    return seconds;
                
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
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            str = str.ToLower();

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static IEnumerable<string> SplitToLines(string stringToSplit, int maxLineLength)
        {
            string[] words = stringToSplit.Split(' ');
            StringBuilder line = new StringBuilder();
            foreach (string word in words)
            {
                if (word.Length + line.Length <= maxLineLength)
                {
                    line.Append(word + " ");
                }
                else
                {
                    if (line.Length > 0)
                    {
                        yield return line.ToString().Trim();
                        line.Clear();
                    }
                    string overflow = word;
                    while (overflow.Length > maxLineLength)
                    {
                        yield return overflow.Substring(0, maxLineLength);
                        overflow = overflow.Substring(maxLineLength);
                    }
                    line.Append(overflow + " ");
                }
            }
            yield return line.ToString().Trim();
        }

        public static string GetTranslation(string id)
        {
            return Labels.GetText(id);
        }

        public static DateTime? GetDate(string datetime)
        {
            DateTime? result = null;

            string[] formats = { "yyyy-MM-dd hh:mm", "yyyy-MM-dd", "yyyy/MM/dd hh:mm", "yyyy/MM/dd", "dd-MM-yyyy hh:mm", "dd-MM-yyyy", "dd/MM/yyyy hh:mm", "dd/MM/yyyy", "d/M/yyyy h:mm:ss tt", "d/M/yyyy h:mm tt",
                     "dd/MM/yyyy hh:mm:ss", "d/M/yyyy h:mm:ss", 
                     "d/M/yyyy hh:mm tt", "d/M/yyyy hh tt", 
                     "d/M/yyyy h:mm", "d/M/yyyy h:mm",
                     "dd/MM/yyyy hh:mm", "dd/M/yyyy hh:mm"};
            

            if (DateTime.TryParseExact(datetime, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime dt))
                result = dt;
            return result;
        }

        public static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static IEnumerable<IList<T>> SplitList<T>(List<T> list, int nSize = 30)
        {
            for (int i = 0; i < list.Count; i += nSize)
            {
                yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
            }
        }
    }
}
