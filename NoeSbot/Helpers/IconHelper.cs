using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Helpers
{
    public static class IconHelper
    {
        public static string One = "1⃣";
        public static string Two = "2⃣";
        public static string Three = "3⃣";
        public static string Four = "4⃣";
        public static string Five = "5⃣";
        public static string Six = "6⃣";
        public static string Seven = "7⃣";
        public static string Eight = "8⃣";
        public static string Nine = "9⃣";
        public static string Ten = "0⃣";
        public static string Eleven = "🇦";
        public static string Twelve = "🇧";
        public static string Thirteen = "🇨";
        public static string Fourteen = "🇩";
        public static string Fifteen = "🇪";
        public static string Sixteen = "🇫";
        public static string Seventeen = "🇬";
        public static string Eightteen = "🇭";
        public static string Nineteen = "🇮";
        public static string Twenty = "🇯";
        public static string ArrowUp = "🔼";
        public static string ArrowDown = "🔽";
        public static string ArrowLeft = "◀";
        public static string ArrowRight = "▶";
        public static string LongArrowLeft = "⬅";
        public static string LongArrowRight = "➡";
        public static string QuestionFull = "❓";
        public static string Question = "❔";
        public static string Bookmark = "🔖";
        public static string Minus = "➖";
        public static string Bell = "🔔";
        public static string BellStop = "🔕";
        public static string Dice = "🎲";

        private static readonly Dictionary<int, string> _numbers = new Dictionary<int, string>
        {
            { 1, One },
            { 2, Two },
            { 3, Three },
            { 4, Four },
            { 5, Five },
            { 6, Six },
            { 7, Seven },
            { 8, Eight },
            { 9, Nine },
            { 10, Ten },
            { 11, Eleven },
            { 12, Twelve },
            { 13, Thirteen },
            { 14, Fourteen },
            { 15, Fifteen },
            { 16, Sixteen },
            { 17, Seventeen },
            { 18, Eightteen },
            { 19, Nineteen },
            { 20, Twenty }
        };

        public static string GetNumberIcon (int index)
        {
            if (_numbers.TryGetValue(index, out string result))
                return result;
            return Ten;
        }

        public static IEmote GetEmote(string name)
        {
            return new Emoji(name);
        }
    }
}
