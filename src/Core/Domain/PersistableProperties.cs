using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automate.Extensions;

namespace Automate.Domain
{
    public class PersistableProperties : Dictionary<string, object>
    {
        public PersistableProperties()
        {
        }

        public PersistableProperties(Dictionary<string, object> properties) : base(properties)
        {
        }

        public Dictionary<string, object> Dictionary => this;

        public void Dehydrate(string name, IEnumerable<byte> bytes)
        {
            Dehydrate(name, (object)bytes);
        }

        public void Dehydrate(string name, IEnumerable<KeyValuePair<string, object>> value)
        {
            Dehydrate(name, (object)value);
        }

        public void Dehydrate(string name, string value)
        {
            Dehydrate(name, (object)value);
        }

        public void Dehydrate(string name, bool value)
        {
            Dehydrate(name, (object)value);
        }

        public void Dehydrate<TPersistable>(string name, TPersistable value)
            where TPersistable : class, IPersistable
        {
            Dehydrate(name, value?.Dehydrate().Dictionary);
        }

        public void Dehydrate<TPersistable>(string name, IEnumerable<TPersistable> items)
            where TPersistable : IPersistable
        {
            var result = items.Safe()
                .Select(item => item.Dehydrate().Dictionary)
                .ToList();
            Dehydrate(name, result);
        }

        public void Dehydrate<TPersistable>(string name, IEnumerable<KeyValuePair<string, TPersistable>> dictionary)
            where TPersistable : IPersistable
        {
            var result =
                dictionary.Safe().ToDictionary(pair => pair.Key, pair => pair.Value.Dehydrate().Dictionary);
            Dehydrate(name, result);
        }

        public void Dehydrate(string name, object value)
        {
            if (value is Enum @enum)
            {
                base.Add(name, @enum.ToString());
                return;
            }

            if (value is string @string)
            {
                base.Add(name, @string);
                return;
            }

            if (value is bool @bool)
            {
                base.Add(name, @bool);
                return;
            }

            if (value is IEnumerable<byte> bytes)
            {
                object value1 = Convert.ToBase64String(bytes.ToArray());
                base.Add(name, value1);
                return;
            }

            if (value is IEnumerable<KeyValuePair<string, object>> dictionary)
            {
                base.Add(name, dictionary);
                return;
            }

            base.Add(name, value);
        }

        public new void Add(string key, object value)
        {
            Dehydrate(key, value);
        }

        public void Add<TPersistable>(string name, TPersistable value)
            where TPersistable : class, IPersistable
        {
            Dehydrate(name, value?.Dehydrate().Dictionary);
        }

        public void Add<TPersistable>(string name, IEnumerable<TPersistable> items)
            where TPersistable : IPersistable
        {
            var result = items.Safe()
                .Select(item => item.Dehydrate().Dictionary)
                .ToList();
            Dehydrate(name, result);
        }

        public void Add<TPersistable>(string name, IEnumerable<KeyValuePair<string, TPersistable>> dictionary)
            where TPersistable : IPersistable
        {
            var result =
                dictionary.Safe().ToDictionary(pair => pair.Key, pair => pair.Value.Dehydrate().Dictionary);
            Dehydrate(name, result);
        }

        public TValue Rehydrate<TValue>(IPersistableFactory factory,
            string propertyName, TValue defaultValue = default)
        {
            factory.GuardAgainstNull(nameof(factory));
            propertyName.GuardAgainstNullOrEmpty(nameof(propertyName));

            if (!ContainsKey(propertyName))
            {
                if (IsObjectDictionary(typeof(TValue)))
                {
                    return (TValue)(object)new Dictionary<string, object>();
                }

                if (IsGenericDictionary(typeof(TValue)))
                {
                    var outputDictionaryType = GetTypeOfGenericDictionary(typeof(TValue));
                    return (TValue)CreateEmptyGenericDictionary(outputDictionaryType);
                }

                if (IsGenericList(typeof(TValue)))
                {
                    var outputListType = GetTypeOfGenericList(typeof(TValue));
                    return (TValue)CreateEmptyGenericList(outputListType);
                }

                if (typeof(TValue) == typeof(byte[]))
                {
                    return (TValue)(object)Array.Empty<byte>();
                }

                return defaultValue;
            }

            var propertyValue = this[propertyName];
            if (propertyValue is TValue value)
            {
                return value;
            }

            if (IsPersistableType(typeof(TValue)))
            {
                var persistableProperty = new PersistableProperties(propertyValue as Dictionary<string, object>);
                var persistable = factory.Rehydrate(typeof(TValue), persistableProperty);
                return (TValue)persistable;
            }

            if (IsGenericDictionary(typeof(TValue)))
            {
                var outputDictionaryType = GetTypeOfGenericDictionary(typeof(TValue));

                if (IsObjectDictionary(propertyValue.GetType()))
                {
                    var propertyDictionaryType = GetTypeOfGenericDictionary(propertyValue.GetType());
                    if (propertyDictionaryType != typeof(object))
                    {
                        throw new NotImplementedException();
                    }

                    var dictionary = CreateEmptyGenericDictionary(outputDictionaryType);
                    var propertyDictionary = propertyValue as Dictionary<string, object>;
                    foreach (var pair in propertyDictionary!)
                    {
                        if (IsPersistableType(outputDictionaryType))
                        {
                            var persistableItem = new PersistableProperties(pair.Value as Dictionary<string, object>);
                            var persistable = factory.Rehydrate(outputDictionaryType, persistableItem);
                            dictionary.Add(pair.Key, persistable);
                        }
                        else if (pair.Value.GetType() == outputDictionaryType)
                        {
                            dictionary.Add(pair.Key, pair.Value);
                        }
                    }

                    return (TValue)dictionary;
                }

                return (TValue)CreateEmptyGenericDictionary(outputDictionaryType);
            }

            if (IsGenericList(typeof(TValue)))
            {
                var outputListType = GetTypeOfGenericList(typeof(TValue));

                if (IsGenericList(propertyValue.GetType()))
                {
                    var propertyListType = GetTypeOfGenericList(propertyValue.GetType());
                    if (propertyListType != typeof(object))
                    {
                        throw new NotImplementedException();
                    }

                    var list = CreateEmptyGenericList(outputListType);
                    var propertyList = propertyValue as List<object>;
                    foreach (var item in propertyList!)
                    {
                        if (IsPersistableType(outputListType))
                        {
                            var persistableItem = new PersistableProperties(item as Dictionary<string, object>);
                            var persistable = factory.Rehydrate(outputListType, persistableItem);
                            list.Add(persistable);
                        }
                        else if (item.GetType() == outputListType)
                        {
                            list.Add(item);
                        }
                    }

                    return (TValue)list;
                }

                return (TValue)CreateEmptyGenericList(outputListType);
            }

            if (typeof(TValue).IsEnum)
            {
                if (propertyValue.Exists())
                {
                    return (TValue)Enum.Parse(typeof(TValue), propertyValue.ToString()!);
                }
            }

            if (typeof(TValue) == typeof(byte[]))
            {
                if (propertyValue.Exists())
                {
                    return (TValue)(object)Convert.FromBase64String((string)propertyValue);
                }
            }

            if (defaultValue.Exists())
            {
                return defaultValue;
            }

            return default;

            bool IsObjectDictionary(Type type)
            {
                return type == typeof(Dictionary<string, object>);
            }

            bool IsGenericDictionary(Type type)
            {
                if (!type.IsGenericType)
                {
                    return false;
                }

                return type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            }

            Type GetTypeOfGenericDictionary(Type type)
            {
                if (!IsGenericDictionary(type))
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericDictionaryNotGeneric);
                }

                var genericArguments = type.GenericTypeArguments;
                if (genericArguments.HasNone())
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericDictionaryNoParameters.Substitute(type));
                }
                if (genericArguments.Length > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericDictionaryTooManyParameters.Substitute(type));
                }

                if (genericArguments.First() != typeof(string))
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericDictionaryNotStringKey.Substitute(type));
                }

                return genericArguments.Last();
            }

            IDictionary CreateEmptyGenericDictionary(Type dictionaryType)
            {
                return (IDictionary)Activator.CreateInstance(
                    typeof(Dictionary<,>).MakeGenericType(typeof(string), dictionaryType));
            }

            bool IsPersistableType(Type type)
            {
                return type.IsAssignableTo(typeof(IPersistable));
            }

            bool IsGenericList(Type type)
            {
                if (!type.IsGenericType)
                {
                    return false;
                }

                return type.GetGenericTypeDefinition() == typeof(List<>);
            }

            Type GetTypeOfGenericList(Type type)
            {
                if (!IsGenericList(type))
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericListNotGeneric);
                }

                var genericArguments = type.GenericTypeArguments;
                if (genericArguments.HasNone())
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericListNoParameters.Substitute(type));
                }
                if (genericArguments.Length > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(type),
                        ExceptionMessages.PersistableProperties_GenericListTooManyParameters.Substitute(type));
                }

                return genericArguments.Single();
            }

            IList CreateEmptyGenericList(Type listedType)
            {
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listedType));
            }
        }
    }
}