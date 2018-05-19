using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoeSbot.Database
{
    public static class Extentions
    {
        public static void AddOrUpdate(this IList<string> collection, IEnumerable<string> items)
        {
            foreach (var item in items.ToList())
                collection.AddOrUpdate2(item);
        }

        private static void AddOrUpdate2(this IList<string> collection, string item)
        {
            if (collection.Any(c => c == item))
                collection.Remove(collection.First(c => c == item));

            collection.Add(item);
        }

        public static List<T> EmptyIfNull<T>(this List<T> list)
        {
            return list ?? new List<T>();
        }
        public static T[] EmptyIfNull<T>(this T[] arr)
        {
            return arr ?? new T[0];
        }
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            foreach (var item in source)
                action(item);

            return source;
        }
    }
}
