using System.Collections.Generic;
using Automate.Common.Domain;

namespace Core.UnitTests.Common
{
    internal class TestPersistable : IPersistable
    {
        public TestPersistable()
        {
            AProperty = "avalue";
            AList = new List<object>();
            APersistableList = new List<TestPersistable>();
            ADictionary = new Dictionary<string, object>();
            APersistableDictionary = new Dictionary<string, TestPersistable>();
        }

        public object AProperty { get; set; }

        public List<object> AList { get; }

        public List<TestPersistable> APersistableList { get; }

        public Dictionary<string, object> ADictionary { get; }

        public Dictionary<string, TestPersistable> APersistableDictionary { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(AProperty), AProperty);
            properties.Dehydrate(nameof(AList), AList);
            properties.Dehydrate(nameof(APersistableList), APersistableList);
            properties.Dehydrate(nameof(ADictionary), ADictionary);
            properties.Dehydrate(nameof(APersistableDictionary), APersistableDictionary);

            return properties;
        }
    }
}