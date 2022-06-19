using System;
using System.Collections;
using System.Collections.Generic;
using Automate.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.UnitTests.Extensions
{
    [Trait("Category", "Unit")]
    public class DictionaryExtensionsSpec
    {
        [Fact]
        public void WhenToObjectDictionaryAndNull_ThenReturnsNull()
        {
            var result = ((object)null).ToObjectDictionary();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenToObjectDictionaryAndIsAlreadyObjectDictionary_ThenReturnsDictionary()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "aname", "avalue" }
            };

            var result = dictionary.ToObjectDictionary();

            result.Should().BeSameAs(dictionary);
        }

        [Fact]
        public void WhenToObjectDictionaryAndIsAlreadyDictionaryInterface_ThenReturnsDictionary()
        {
            var dictionary = new TestDictionary
            {
                { "aname", "avalue" }
            };

            var result = dictionary.ToObjectDictionary();

            result.Count.Should().Be(1);
            result.Should().ContainKey("aname").WhoseValue.Should().Be("avalue");
        }

        [Fact]
        public void WhenToObjectDictionaryAndIsAlreadyStringDictionary_ThenReturnsDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "aname", "avalue" }
            };

            var result = dictionary.ToObjectDictionary();

            result.Count.Should().Be(1);
            result.Should().ContainKey("aname").WhoseValue.Should().Be("avalue");
        }

        [Fact]
        public void WhenToObjectDictionaryAndIsObjectWithNoProperties_ThenReturnsEmptyDictionary()
        {
            var @object = new { };

            var result = @object.ToObjectDictionary();

            result.Count.Should().Be(0);
        }

        [Fact]
        public void WhenToObjectDictionaryAndIsObjectWithProperties_ThenReturnsDictionary()
        {
            var date = DateTime.UtcNow;
            var @object = new
            {
                aname1 = "avalue",
                aname2 = 25,
                aname3 = true,
                aname4 = date
            };

            var result = @object.ToObjectDictionary();

            result.Count.Should().Be(4);
            result.Should().ContainKey("aname1").WhoseValue.Should().Be("avalue");
            result.Should().ContainKey("aname2").WhoseValue.Should().Be(25);
            result.Should().ContainKey("aname3").WhoseValue.Should().Be(true);
            result.Should().ContainKey("aname4").WhoseValue.Should().Be(date);
        }

        [Fact]
        public void WhenFromObjectDictionaryWithNull_ThenReturnsNull()
        {
            var result = ((IEnumerable<KeyValuePair<string, object>>)null).FromObjectDictionary<object>();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenFromObjectDictionaryWithDictionaryAsDictionary_ThenReturnsDictionary()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "aname", "avalue" }
            };

            var result = dictionary.FromObjectDictionary<Dictionary<string, object>>();

            result.Should().BeSameAs(dictionary);
        }

        [Fact]
        public void WhenFromObjectDictionaryWithDictionaryAsObject_ThenReturnsObject()
        {
            var date = DateTime.UtcNow;
            var dictionary = new Dictionary<string, object>
            {
                { "aname", "avalue1" },
                { "AStringProperty", "avalue2" },
                { "AnIntegerProperty", 25 },
                { "ADateTimeProperty", date }
            };

            var result = dictionary.FromObjectDictionary<TestObject>();

            result.AStringProperty.Should().Be("avalue2");
            result.AnIntegerProperty.Should().Be(25);
            result.ADateTimeProperty.Should().Be(date);
        }
    }

    internal class TestObject
    {
        public string AStringProperty { get; set; }

        public int AnIntegerProperty { get; set; }

        public DateTime ADateTimeProperty { get; set; }
    }

    internal class TestDictionary : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> inner;

        public TestDictionary()
        {
            this.inner = new Dictionary<string, object>();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public int Count => this.inner.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            this.inner.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        public object this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<string> Keys => this.inner.Keys;

        public ICollection<object> Values => this.inner.Values;
    }
}