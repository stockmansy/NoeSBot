using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoeSbot.Extensions
{
    static class ExtensionMethods
    {
        public static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }

        public static bool HasCharPrefix(this IUserMessage msg, char[] chars, ref int argPos)
        {
            var text = msg.Content;
            if (!string.IsNullOrEmpty(text) && chars.Contains(text[0]))
            {
                argPos = 1;
                return true;
            }
            return false;
        }

        public static bool NotNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }
    }
}
