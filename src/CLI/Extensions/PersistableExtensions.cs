using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Automate.CLI.Domain;

namespace Automate.CLI.Extensions
{
    internal static class PersistableExtensions
    {
        public static string ToJson(this IPersistable persistable, IPersistableFactory persistableFactory)
        {
            persistable.GuardAgainstNull(nameof(persistable));

            var properties = persistable.Dehydrate();
            var dictionary = properties.Dictionary;
            return JsonSerializer.Serialize(dictionary, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new ListConverter(), new DictionaryConverter() }
            });
        }

        public static TPersistable FromJson<TPersistable>(this string json, IPersistableFactory factory)
            where TPersistable : IPersistable
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
            {
                Converters = { new ListConverter(), new DictionaryConverter() }
            });
            var properties = new PersistableProperties(dictionary);
            return (TPersistable)factory.Rehydrate<TPersistable>(properties);
        }

        private class ListConverter : JsonConverter<List<object>>
        {
            public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                var list = new List<object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return list;
                    }

                    if (reader.TokenType != JsonTokenType.StartObject
                        && reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException();
                    }

                    object value = null;
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        value = ReadWithConverter<Dictionary<string, object>>(ref reader, options);
                    }
                    else if (reader.TokenType == JsonTokenType.String)
                    {
                        value = reader.GetString();
                    }

                    list.Add(value);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();

                foreach (var item in value)
                {
                    if (item is Dictionary<string, object> dictionary)
                    {
                        WriteWithConverter(writer, options, dictionary);
                    }
                    else
                    {
                        WriteScalarValue(writer, options, item);
                    }
                }

                writer.WriteEndArray();
            }

            private static object ReadWithConverter<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (valueConverter.Exists())
                {
                    return valueConverter.Read(ref reader, typeof(TValue), options)!;
                }

                return default;
            }

            private static void WriteScalarValue(Utf8JsonWriter writer, JsonSerializerOptions options, object item)
            {
                JsonSerializer.Serialize(writer, item, options);
            }

            private static void WriteWithConverter<TValue>(Utf8JsonWriter writer, JsonSerializerOptions options, TValue value)
            {
                var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (valueConverter.Exists())
                {
                    valueConverter.Write(writer, value, options);
                }
            }
        }

        private class DictionaryConverter : JsonConverter<Dictionary<string, object>>
        {
            public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var dictionary = new Dictionary<string, object>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return dictionary;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    var propertyName = reader.GetString();

                    reader.Read();

                    object value;
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        value = ReadWithConverter<List<object>>(ref reader, options);
                    }
                    else if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        value = ReadWithConverter<Dictionary<string, object>>(ref reader, options);
                    }
                    else
                    {
                        value = ReadScalarValue(ref reader);
                    }

                    dictionary.Add(propertyName!, value);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (var item in value)
                {
                    var propertyName = item.Key;

                    if (item.Value is Dictionary<string, Dictionary<string, object>> dictionaryOfDictionary)
                    {
                        if (dictionaryOfDictionary.HasNone())
                        {
                            continue;
                        }

                        WriteWithConverter(writer, options, propertyName,
                            dictionaryOfDictionary.ToDictionary(pair => pair.Key, pair => (object)pair.Value));
                    }
                    else if (item.Value is Dictionary<string, object> dictionary)
                    {
                        if (dictionary.HasNone())
                        {
                            continue;
                        }

                        WriteWithConverter(writer, options, propertyName, dictionary);
                    }
                    else if (item.Value is List<Dictionary<string, object>> listOfDictionary)
                    {
                        if (listOfDictionary.HasNone())
                        {
                            continue;
                        }

                        WriteWithConverter(writer, options, propertyName, listOfDictionary.Select(dic => (object)dic).ToList());
                    }
                    else if (item.Value is List<object> list)
                    {
                        if (list.HasNone())
                        {
                            continue;
                        }

                        WriteWithConverter(writer, options, propertyName, list);
                    }
                    else if (item.Value.Exists())
                    {
                        WriteScalarValue(writer, options, propertyName, item);
                    }
                }

                writer.WriteEndObject();
            }

            private static object ReadScalarValue(ref Utf8JsonReader reader)
            {
                return reader.TokenType switch
                {
                    JsonTokenType.True => true,
                    JsonTokenType.False => false,
                    JsonTokenType.Number when reader.TryGetInt64(out var l) => l,
                    JsonTokenType.Number => reader.GetDouble(),
                    JsonTokenType.String when reader.TryGetDateTime(out var datetime) => datetime,
                    JsonTokenType.String => reader.GetString()!,
                    _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
                };
            }

            private static object ReadWithConverter<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (valueConverter.Exists())
                {
                    return valueConverter.Read(ref reader, typeof(TValue), options)!;
                }

                return default;
            }

            private static void WriteScalarValue(Utf8JsonWriter writer, JsonSerializerOptions options, string propertyName, KeyValuePair<string, object> item)
            {
                writer.WritePropertyName
                    (options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                JsonSerializer.Serialize(writer, item.Value, options);
            }

            private static void WriteWithConverter<TValue>(Utf8JsonWriter writer, JsonSerializerOptions options, string propertyName, TValue value)
            {
                var valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (valueConverter.Exists())
                {
                    writer.WritePropertyName
                        (options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                    valueConverter.Write(writer, value, options);
                }
            }
        }
    }
}