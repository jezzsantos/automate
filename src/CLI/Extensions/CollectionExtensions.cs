using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace automate.Extensions
{
    internal static class CollectionExtensions
    {
        public static List<T> ToListSafe<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Safe().ToList();
        }

        public static IEnumerable<T> Safe<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static string Join<T>(this IEnumerable<T> values)
        {
            return values.Join(",");
        }

        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
            var stringBuilder = new StringBuilder();
            foreach (var value in values)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(separator);
                }

                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }

        public static string SafeJoin(this IEnumerable<string> values, string separator)
        {
            if (values.NotExists())
            {
                return null;
            }
            return values.Join(separator);
        }

        public static string[] SafeSplit(this string value, string[] delimiters,
            StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (!value.HasValue())
            {
                return new string[]
                {
                };
            }

            return value.Split(delimiters, options);
        }

        public static string[] SafeSplit(this string value, string delimiter,
            StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return value.SafeSplit(new[]
            {
                delimiter
            }, options);
        }

        public static Dictionary<TKey, TValue> AsDictionary<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
        {
            return readOnlyDictionary
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
        {
            return readOnlyDictionary
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static bool HasAny<T>(this IEnumerable<T> collection)
        {
            return collection.Safe().Any();
        }

        public static bool HasNone<T>(this IEnumerable<T> collection)
        {
            return !HasAny(collection);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}