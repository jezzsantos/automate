﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceStack.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Automate.CLI.Extensions
{
    internal static class JsonConversions
    {
        public static string ToJson<T>(this T instance) where T : new()
        {
            using (var scope = JsConfig.BeginScope())
            {
                scope.AssumeUtc = true;
                scope.AlwaysUseUtc = true;
                scope.Indent = true;
                return ServiceStack.StringExtensions.ToJson(instance);
            }
        }

        public static T FromJson<T>(this string json) where T : new()
        {
            using (var scope = JsConfig.BeginScope())
            {
                scope.AssumeUtc = true;
                scope.AlwaysUseUtc = true;
                return ServiceStack.StringExtensions.FromJson<T>(json);
            }
        }

        public static Dictionary<string, object> FromJson(this string value)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DictionaryStringObjectJsonConverter());
            return JsonSerializer.Deserialize<Dictionary<string, object>>(value, options);
        }
    }

    internal class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
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
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                dictionary.Add(propertyName, ExtractValue(ref reader, options));
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }

        private object ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    if (reader.TryGetDateTime(out var date))
                    {
                        return date;
                    }
                    return reader.GetString();

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var result))
                    {
                        return result;
                    }
                    return reader.GetDecimal();

                case JsonTokenType.StartObject:
                    return Read(ref reader, null, options);

                case JsonTokenType.StartArray:
                    var list = new List<object>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ExtractValue(ref reader, options));
                    }
                    return list;

                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }
    }
}