using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automate.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToObjectDictionary(this object instance)
        {
            if (instance.NotExists())
            {
                return null;
            }

            if (instance is Dictionary<string, object> alreadyDict)
            {
                return alreadyDict;
            }

            if (instance is IDictionary<string, object> interfaceDict)
            {
                return new Dictionary<string, object>(interfaceDict);
            }

            var to = new Dictionary<string, object>();
            if (instance is Dictionary<string, string> stringDict)
            {
                foreach (var entry in stringDict)
                {
                    to[entry.Key] = entry.Value;
                }
                return to;
            }

            if (instance is IDictionary iDictionary)
            {
                foreach (var key in iDictionary.Keys)
                {
                    to[key.ToString()!] = iDictionary[key];
                }
                return to;
            }

            if (instance is NameValueCollection nameValueCollection)
            {
                for (var index = 0; index < nameValueCollection.Count; index++)
                {
                    to[nameValueCollection.GetKey(index)!] = nameValueCollection.Get(index);
                }
                return to;
            }

            if (instance is IEnumerable<KeyValuePair<string, object>> objKeyValuePairs)
            {
                foreach (var kvp in objKeyValuePairs)
                {
                    to[kvp.Key] = kvp.Value;
                }
                return to;
            }
            if (instance is IEnumerable<KeyValuePair<string, string>> stringKeyValuePairs)
            {
                foreach (var kvp in stringKeyValuePairs)
                {
                    to[kvp.Key] = kvp.Value;
                }
                return to;
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new DictionaryStringObjectJsonConverter());
            var json = JsonSerializer.Serialize(instance, options);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
        }

        public static T FromObjectDictionary<T>(this IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values.NotExists())
            {
                return default;
            }

            var type = typeof(T);
            var alreadyDict = typeof(IEnumerable<KeyValuePair<string, object>>).IsAssignableFrom(type);
            if (alreadyDict)
            {
                return (T)values;
            }

            var instance = values.ToDictionary(pair => pair.Key, pair => pair.Value);
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(instance, options);
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}