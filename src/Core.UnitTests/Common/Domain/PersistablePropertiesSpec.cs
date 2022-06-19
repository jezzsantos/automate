using System;
using System.Collections.Generic;
using Automate.Common.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace Core.UnitTests.Common.Domain
{
    [Trait("Category", "Unit")]
    public class PersistablePropertiesSpec
    {
        private readonly Mock<IPersistableFactory> factory;

        public PersistablePropertiesSpec()
        {
            this.factory = new Mock<IPersistableFactory>();
        }

        [Fact]
        public void WhenDehydrateAndScalarString_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", "avalue");

            properties.Should().ContainSingle("avalue");
        }

        [Fact]
        public void WhenDehydrateAndScalarBool_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", true);

            properties.Should().ContainSingle(x => true);
        }

        [Fact]
        public void WhenDehydrateAndScalarByteArray_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", new byte[] { 0x00, 0x01, 0x02 });

            properties.Should().ContainSingle(Convert.ToBase64String(new byte[] { 0x00, 0x01, 0x02 }));
        }

        [Fact]
        public void WhenDehydrateAndScalarEnum_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", TestEnum.Two);

            properties.Should().ContainSingle(TestEnum.Two.ToString());
        }

        [Fact]
        public void WhenDehydrateAndDictionaryOfObject_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", new Dictionary<string, object>
            {
                { "aname1", "avalue1" },
                { "aname2", "avalue2" },
                { "aname3", "avalue3" }
            });

            properties.Should().ContainSingle(pair => pair.Key == "aname").Which.Value
                .As<Dictionary<string, object>>().Should().ContainValues("avalue1", "avalue2", "avalue3");
        }

        [Fact]
        public void WhenDehydrateAndPersistable_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", new TestPersistable());

            properties["aname"].Should().BeEquivalentTo(new Dictionary<string, object>
            {
                { nameof(TestPersistable.AProperty), "avalue" },
                { nameof(TestPersistable.AList), new List<string>() },
                { nameof(TestPersistable.APersistableList), new List<object>() },
                { nameof(TestPersistable.ADictionary), new Dictionary<string, object>() },
                { nameof(TestPersistable.APersistableDictionary), new Dictionary<string, TestPersistable>() }
            });
        }

        [Fact]
        public void WhenDehydrateAndListOfScalar_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", new List<string> { "avalue" });

            properties["aname"].Should().BeEquivalentTo(new List<string> { "avalue" });
        }

        [Fact]
        public void WhenDehydrateAndListOfPersistable_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", new List<TestPersistable>
            {
                new()
            });

            properties["aname"].Should().BeEquivalentTo(new List<Dictionary<string, object>>
            {
                new()
                {
                    { nameof(TestPersistable.AProperty), "avalue" },
                    { nameof(TestPersistable.AList), new List<string>() },
                    { nameof(TestPersistable.APersistableList), new List<object>() },
                    { nameof(TestPersistable.ADictionary), new Dictionary<string, object>() },
                    { nameof(TestPersistable.APersistableDictionary), new Dictionary<string, TestPersistable>() }
                }
            });
        }

        [Fact]
        public void WhenDehydrateAndDictionaryOfPersistable_ThenAdds()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate("aname", new Dictionary<string, TestPersistable>
            {
                { "aname1", new TestPersistable() },
                { "aname2", new TestPersistable() },
                { "aname3", new TestPersistable() }
            });

            properties["aname"].Should().BeEquivalentTo(new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "aname1", new Dictionary<string, object>
                    {
                        { nameof(TestPersistable.AProperty), "avalue" },
                        { nameof(TestPersistable.AList), new List<string>() },
                        { nameof(TestPersistable.APersistableList), new List<object>() },
                        { nameof(TestPersistable.ADictionary), new Dictionary<string, object>() },
                        { nameof(TestPersistable.APersistableDictionary), new Dictionary<string, TestPersistable>() }
                    }
                },
                {
                    "aname2", new Dictionary<string, object>
                    {
                        { nameof(TestPersistable.AProperty), "avalue" },
                        { nameof(TestPersistable.AList), new List<string>() },
                        { nameof(TestPersistable.APersistableList), new List<object>() },
                        { nameof(TestPersistable.ADictionary), new Dictionary<string, object>() },
                        { nameof(TestPersistable.APersistableDictionary), new Dictionary<string, TestPersistable>() }
                    }
                },
                {
                    "aname3", new Dictionary<string, object>
                    {
                        { nameof(TestPersistable.AProperty), "avalue" },
                        { nameof(TestPersistable.AList), new List<string>() },
                        { nameof(TestPersistable.APersistableList), new List<object>() },
                        { nameof(TestPersistable.ADictionary), new Dictionary<string, object>() },
                        { nameof(TestPersistable.APersistableDictionary), new Dictionary<string, TestPersistable>() }
                    }
                }
            });
        }

        [Fact]
        public void WhenRehydrateAndPropertyNameIsNull_ThenReturnsThrows()
        {
            new PersistableProperties()
                .Invoking(x => x.Rehydrate<string>(this.factory.Object, null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenRehydrateAndMissingAndNoDefaultValue_ThenReturnsTypeDefault()
        {
            var result = new PersistableProperties()
                .Rehydrate<int>(this.factory.Object, "apropertyname");

            result.Should().Be(default);
        }

        [Fact]
        public void WhenRehydrateAndMissingAndDefaultValue_ThenReturnsDefaultValue()
        {
            var result = new PersistableProperties()
                .Rehydrate(this.factory.Object, "apropertyname", "adefaultvalue");

            result.Should().BeOfType<string>();
            result.Should().Be("adefaultvalue");
        }

        [Fact]
        public void WhenRehydrate_ThenReturnsValue()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", "avalue" }
            }.Rehydrate(this.factory.Object, "apropertyname", "adefaultvalue");

            result.Should().BeOfType<string>();
            result.Should().Be("avalue");
        }

        [Fact]
        public void WhenRehydrateForScalarAndWrongType_ThenReturnsNull()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", "avalue" }
            }.Rehydrate<int>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(int));
            result.Should().Be(default);
        }

        [Fact]
        public void WhenRehydrateAndWrongTypeAndConvertible_ThenReturnsNull()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", "25" }
            }.Rehydrate<int>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(int));
            result.Should().Be(default);
        }

        [Fact]
        public void WhenRehydrateScalar_ThenReturnsValue()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", "avalue" }
            }.Rehydrate<string>(this.factory.Object, "apropertyname");

            result.Should().BeOfType<string>();
            result.Should().Be("avalue");
        }

        [Fact]
        public void WhenRehydrateBytesAndMissing_ThenReturnsEmptyArray()
        {
            var result = new PersistableProperties()
                .Rehydrate<byte[]>(this.factory.Object, "apropertyname");

            result.Should().BeOfType<byte[]>();
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateBytes_ThenReturnsValue()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", Convert.ToBase64String(new byte[] { 0x01, 0x01, 0x02 }) }
            }.Rehydrate<byte[]>(this.factory.Object, "apropertyname");

            result.Should().BeOfType<byte[]>();
            result.Should().Equal(0x01, 0x01, 0x02);
        }

        [Fact]
        public void WhenRehydrateEnumAndMissing_ThenReturnsDefaultEnum()
        {
            var result = new PersistableProperties()
                .Rehydrate<TestEnum>(this.factory.Object, "apropertyname");

            result.Should().Be(TestEnum.None);
        }

        [Fact]
        public void WhenRehydrateEnum_ThenReturnsValue()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", TestEnum.Two.ToString() }
            }.Rehydrate<TestEnum>(this.factory.Object, "apropertyname");

            result.Should().Be(TestEnum.Two);
        }

        [Fact]
        public void WhenRehydrateForPersistable_ThenReturnsValue()
        {
            this.factory.Setup(pf => pf.Rehydrate(typeof(TestPersistable),
                    It.IsAny<PersistableProperties>()))
                .Returns(
                    (Type _, PersistableProperties props) =>
                        new TestPersistable { AProperty = props["AProperty"] });

            var result = new PersistableProperties
            {
                {
                    "apropertyname", new Dictionary<string, object>
                    {
                        { nameof(TestPersistable.AProperty), "avalue1" }
                    }
                }
            }.Rehydrate<TestPersistable>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(TestPersistable));
            result.AProperty.Should().Be("avalue1");
        }

        [Fact]
        public void WhenRehydrateForListOfScalarAndWrongType_ThenReturnsEmptyList()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", "avalue" }
            }.Rehydrate<List<string>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<string>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForMissingListOfScalar_ThenReturnsEmptyList()
        {
            var result = new PersistableProperties()
                .Rehydrate<List<string>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<string>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForEmptyListOfScalar_ThenReturnsEmptyList()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", new List<object>() }
            }.Rehydrate<List<string>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<string>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForListOfScalar_ThenReturnsList()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", new List<object> { "avalue" } }
            }.Rehydrate<List<string>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<string>));
            result.Should().ContainSingle("avalue");
        }

        [Fact]
        public void WhenRehydrateForListOfScalarAndWrongType_ThenReturnsList()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", new List<object> { 25 } }
            }.Rehydrate<List<string>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<string>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForMissingListOfPersistable_ThenReturnsEmptyList()
        {
            var result = new PersistableProperties()
                .Rehydrate<List<TestPersistable>>(this.factory.Object,
                    "apropertyname");

            result.Should().BeOfType(typeof(List<TestPersistable>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForEmptyListOfPersistable_ThenReturnsEmptyList()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", new List<object>() }
            }.Rehydrate<List<TestPersistable>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<TestPersistable>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForListOfPersistable_ThenReturnsList()
        {
            this.factory.Setup(pf => pf.Rehydrate(typeof(TestPersistable),
                    It.IsAny<PersistableProperties>()))
                .Returns(
                    (Type _, PersistableProperties props) =>
                        new TestPersistable { AProperty = props["AProperty"] });

            var result = new PersistableProperties
            {
                {
                    "apropertyname", new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { nameof(TestPersistable.AProperty), "avalue1" }
                        },
                        new Dictionary<string, object>
                        {
                            { nameof(TestPersistable.AProperty), "avalue2" }
                        },
                        new Dictionary<string, object>
                        {
                            { nameof(TestPersistable.AProperty), "avalue3" }
                        }
                    }
                }
            }.Rehydrate<List<TestPersistable>>(this.factory.Object, "apropertyname");

            result.Should().BeOfType(typeof(List<TestPersistable>));
            result.Should().Contain(obj => (string)obj.AProperty == "avalue1");
            result.Should().Contain(obj => (string)obj.AProperty == "avalue2");
            result.Should().Contain(obj => (string)obj.AProperty == "avalue3");
        }

        [Fact]
        public void WhenRehydrateForMissingDictionaryOfObject_ThenReturnsEmptyDictionary()
        {
            var result = new PersistableProperties()
                .Rehydrate<Dictionary<string, object>>(this.factory.Object,
                    "apropertyname");

            result.Should().BeOfType(typeof(Dictionary<string, object>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForEmptyDictionaryOfObject_ThenReturnsEmptyDictionary()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", new Dictionary<string, object>() }
            }.Rehydrate<Dictionary<string, object>>(this.factory.Object,
                "apropertyname");

            result.Should().BeOfType(typeof(Dictionary<string, object>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForDictionaryOfObject_ThenReturnsDictionary()
        {
            var result = new PersistableProperties
            {
                {
                    "apropertyname", new Dictionary<string, object>
                    {
                        { "aname1", "avalue" },
                        { "aname2", 25 },
                        { "aname3", true }
                    }
                }
            }.Rehydrate<Dictionary<string, object>>(this.factory.Object,
                "apropertyname");

            result.Should().BeOfType(typeof(Dictionary<string, object>));
            result.Should().ContainValues("avalue", 25, true);
        }

        [Fact]
        public void WhenRehydrateForMissingDictionaryOfPersistable_ThenReturnsEmptyDictionary()
        {
            var result = new PersistableProperties()
                .Rehydrate<Dictionary<string, TestPersistable>>(this.factory.Object,
                    "apropertyname");

            result.Should().BeOfType(typeof(Dictionary<string, TestPersistable>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForEmptyDictionaryOfPersistable_ThenReturnsEmptyDictionary()
        {
            var result = new PersistableProperties
            {
                { "apropertyname", new Dictionary<string, object>() }
            }.Rehydrate<Dictionary<string, TestPersistable>>(this.factory.Object,
                "apropertyname");

            result.Should().BeOfType(typeof(Dictionary<string, TestPersistable>));
            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenRehydrateForDictionaryOfPersistable_ThenReturnsDictionary()
        {
            this.factory.Setup(pf => pf.Rehydrate(typeof(TestPersistable),
                    It.IsAny<PersistableProperties>()))
                .Returns(
                    (Type _, PersistableProperties props) =>
                        new TestPersistable { AProperty = props["AProperty"] });

            var result = new PersistableProperties
            {
                {
                    "apropertyname", new Dictionary<string, object>
                    {
                        {
                            "aname1", new Dictionary<string, object>
                            {
                                { nameof(TestPersistable.AProperty), "avalue" }
                            }
                        },
                        {
                            "aname2", new Dictionary<string, object>
                            {
                                { nameof(TestPersistable.AProperty), 25 }
                            }
                        },
                        {
                            "aname3", new Dictionary<string, object>
                            {
                                { nameof(TestPersistable.AProperty), true }
                            }
                        }
                    }
                }
            }.Rehydrate<Dictionary<string, TestPersistable>>(this.factory.Object,
                "apropertyname");

            result.Should().BeOfType(typeof(Dictionary<string, TestPersistable>));
            result.Should().Contain(pair => pair.Key == "aname1").Which.Value.AProperty.Should().Be("avalue");
            result.Should().Contain(pair => pair.Key == "aname2").Which.Value.AProperty.Should().Be(25);
            result.Should().Contain(pair => pair.Key == "aname3").Which.Value.AProperty.Should().Be(true);
        }
    }

    internal enum TestEnum
    {
        None = 0,
        One = 1,
        Two = 2
    }
}