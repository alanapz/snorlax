using System;
using System.Collections.Generic;

namespace Snorlax.Utils
{
    public static class AppUtils
    {
        public static V GetOrCreate<K, V>(this Dictionary<K, V> source, K key, Func<V> factory) where K : notnull
        {
            V value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            value = factory();
            source.Add(key, value);
            return value;
        }

        public static void RemoveIf<K, V>(this Dictionary<K, V> source, Predicate<KeyValuePair<K, V>> predicate) where K : notnull
        {
            foreach (KeyValuePair<K, V> entry in new List<KeyValuePair<K, V>>(source))
            {
                if (predicate(entry))
                {
                    source.Remove(entry.Key);
                }
            }
        }

        public static string ToPrettyString<T>(this IEnumerable<T> source)
        {
            return $"[{string.Join(',', source)}]";
        }

        public static string Truncate(this string? source, int length)
        {
            if (source == null)
            {
                return "";
            }
            if (source.Length < length)
            {
                return source;
            }
            return $"{source.Substring(0, length - 5)} ...";
        }
    }
}
