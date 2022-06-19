using System;
using System.Collections.Generic;
using Automate;
using Automate.Domain;
using Automate.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Core.UnitTests.Extensions
{
    [Trait("Category", "Unit")]
    public class PersistenceExtensionsSpec
    {
        private readonly Mock<IPersistableFactory> factory;

        public PersistenceExtensionsSpec()
        {
            this.factory = new Mock<IPersistableFactory>();
        }

        [Fact]
        public void WhenToJsonAndNull_ThenThrows()
        {
            FluentActions.Invoking(() => PersistableExtensions.ToJson(null, this.factory.Object))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenToJsonWithScalarProperty_ThenReturnsJson()
        {
            var result = new TestPersistable().ToJson(this.factory.Object);

            result.Should().Be($"{{{Environment.NewLine}" +
                               $"  \"AProperty\": \"avalue\"{Environment.NewLine}" +
                               "}");
        }

        [Fact]
        public void WhenToJsonWithListOfScalarProperty_ThenReturnsJson()
        {
            var result = new TestPersistable
            {
                AList = { "alistvalue" }
            }.ToJson(this.factory.Object);

            result.Should().Be($"{{{Environment.NewLine}" +
                               $"  \"AProperty\": \"avalue\",{Environment.NewLine}" +
                               $"  \"AList\": [{Environment.NewLine}" +
                               $"    \"alistvalue\"{Environment.NewLine}" +
                               $"  ]{Environment.NewLine}" +
                               "}");
        }

        [Fact]
        public void WhenToJsonWithListOfPersistableProperty_ThenReturnsJson()
        {
            var result = new TestPersistable
            {
                APersistableList = { new TestPersistable() }
            }.ToJson(this.factory.Object);

            result.Should().Be($"{{{Environment.NewLine}" +
                               $"  \"AProperty\": \"avalue\",{Environment.NewLine}" +
                               $"  \"APersistableList\": [{Environment.NewLine}" +
                               $"    {{{Environment.NewLine}" +
                               $"      \"AProperty\": \"avalue\"{Environment.NewLine}" +
                               $"    }}{Environment.NewLine}" +
                               $"  ]{Environment.NewLine}" +
                               "}");
        }

        [Fact]
        public void WhenToJsonWithDictionaryOfObjectProperty_ThenReturnsJson()
        {
            var result = new TestPersistable
            {
                ADictionary = { { "aname1", "avalue" }, { "aname2", 25 }, { "aname3", true } }
            }.ToJson(this.factory.Object);

            result.Should().Be($"{{{Environment.NewLine}" +
                               $"  \"AProperty\": \"avalue\",{Environment.NewLine}" +
                               $"  \"ADictionary\": {{{Environment.NewLine}" +
                               $"    \"aname1\": \"avalue\",{Environment.NewLine}" +
                               $"    \"aname2\": 25,{Environment.NewLine}" +
                               $"    \"aname3\": true{Environment.NewLine}" +
                               $"  }}{Environment.NewLine}" +
                               "}");
        }

        [Fact]
        public void WhenToJsonWithDictionaryOfPersistableProperty_ThenReturnsJson()
        {
            var result = new TestPersistable
            {
                APersistableDictionary =
                {
                    { "aname1", new TestPersistable() }, { "aname2", new TestPersistable() },
                    { "aname3", new TestPersistable() }
                }
            }.ToJson(this.factory.Object);

            result.Should().Be($"{{{Environment.NewLine}" +
                               $"  \"AProperty\": \"avalue\",{Environment.NewLine}" +
                               $"  \"APersistableDictionary\": {{{Environment.NewLine}" +
                               $"    \"aname1\": {{{Environment.NewLine}" +
                               $"      \"AProperty\": \"avalue\"{Environment.NewLine}" +
                               $"    }},{Environment.NewLine}" +
                               $"    \"aname2\": {{{Environment.NewLine}" +
                               $"      \"AProperty\": \"avalue\"{Environment.NewLine}" +
                               $"    }},{Environment.NewLine}" +
                               $"    \"aname3\": {{{Environment.NewLine}" +
                               $"      \"AProperty\": \"avalue\"{Environment.NewLine}" +
                               $"    }}{Environment.NewLine}" +
                               $"  }}{Environment.NewLine}" +
                               "}");
        }

        [Fact]
        public void WhenFromJsonAndJsonIsNull_ThenThrows()
        {
            FluentActions.Invoking(() =>
                    PersistableExtensions.FromJson<TestPersistable>(null,
                        Mock.Of<IPersistableFactory>()))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.PersistableExtensions_FromJson_NoJson);
        }

        [Fact]
        public void WhenFromJsonWithEmptyJson_ThenRehydratesNone()
        {
            var persistable = new TestPersistable();
            this.factory.Setup(pf =>
                    pf.Rehydrate<TestPersistable>(It.IsAny<PersistableProperties>()))
                .Returns(persistable);

            var result = "{}".FromJson<TestPersistable>(this.factory.Object);

            result.Should().Be(persistable);
            this.factory.Verify(pf => pf.Rehydrate<TestPersistable>(It.Is<PersistableProperties>(props =>
                props.Count == 0
            )));
        }

        [Fact]
        public void WhenFromJsonWithAScalarInJson_ThenRehydratesScalar()
        {
            var persistable = new TestPersistable();
            this.factory.Setup(pf =>
                    pf.Rehydrate<TestPersistable>(It.IsAny<PersistableProperties>()))
                .Returns(persistable);

            var result = $"{{\"{nameof(TestPersistable.AProperty)}\":\"anewvalue\"}}"
                .FromJson<TestPersistable>(this.factory.Object);

            result.Should().Be(persistable);
            this.factory.Verify(pf => pf.Rehydrate<TestPersistable>(It.Is<PersistableProperties>(props =>
                props.Count == 1
                && (string)props[$"{nameof(TestPersistable.AProperty)}"] == "anewvalue"
            )));
        }

        [Fact]
        public void WhenFromJsonWithAListOfScalarInJson_ThenRehydratesList()
        {
            var persistable = new TestPersistable();
            this.factory.Setup(pf =>
                    pf.Rehydrate<TestPersistable>(It.IsAny<PersistableProperties>()))
                .Returns(persistable);

            var json = $"{{\"{nameof(TestPersistable.AList)}\":[" +
                       "    \"avalue1\"," +
                       "    \"avalue2\"," +
                       "    \"avalue3\"" +
                       "]}";
            var result = json.FromJson<TestPersistable>(this.factory.Object);

            result.Should().Be(persistable);
            this.factory.Verify(pf => pf.Rehydrate<TestPersistable>(It.Is<PersistableProperties>(props =>
                props.Count == 1
                && (string)((List<object>)props[$"{nameof(TestPersistable.AList)}"])[0] == "avalue1"
                && (string)((List<object>)props[$"{nameof(TestPersistable.AList)}"])[1] == "avalue2"
                && (string)((List<object>)props[$"{nameof(TestPersistable.AList)}"])[2] == "avalue3"
            )));
        }

        [Fact]
        public void WhenFromJsonWithAListOfPersistableInJson_ThenRehydratesList()
        {
            var persistable = new TestPersistable();
            this.factory.Setup(pf =>
                    pf.Rehydrate<TestPersistable>(It.IsAny<PersistableProperties>()))
                .Returns(persistable);

            var json = $"{{\"{nameof(TestPersistable.APersistableList)}\":[" +
                       $"    {{\"{nameof(TestPersistable.AProperty)}\":\"avalue1\"}}," +
                       $"    {{\"{nameof(TestPersistable.AProperty)}\":\"avalue2\"}}," +
                       $"    {{\"{nameof(TestPersistable.AProperty)}\":\"avalue3\"}}" +
                       "]}";
            var result = json.FromJson<TestPersistable>(this.factory.Object);

            result.Should().Be(persistable);

            this.factory.Verify(pf => pf.Rehydrate<TestPersistable>(It.Is<PersistableProperties>(props =>
                props.Count == 1
                && (string)((Dictionary<string, object>)((List<object>)props[
                    $"{nameof(TestPersistable.APersistableList)}"])[0])[nameof(TestPersistable.AProperty)] ==
                "avalue1"
                && (string)((Dictionary<string, object>)((List<object>)props[
                    $"{nameof(TestPersistable.APersistableList)}"])[1])[nameof(TestPersistable.AProperty)] ==
                "avalue2"
                && (string)((Dictionary<string, object>)((List<object>)props[
                    $"{nameof(TestPersistable.APersistableList)}"])[2])[nameof(TestPersistable.AProperty)] ==
                "avalue3"
            )));
        }

        [Fact]
        public void WhenFromJsonWithADictionaryOfObjectInJson_ThenRehydratesDictionary()
        {
            var persistable = new TestPersistable();
            this.factory.Setup(pf =>
                    pf.Rehydrate<TestPersistable>(It.IsAny<PersistableProperties>()))
                .Returns(persistable);

            var json = $"{{\"{nameof(TestPersistable.ADictionary)}\":{{" +
                       "    \"aname1\":\"avalue1\"," +
                       "    \"aname2\":\"avalue2\"," +
                       "    \"aname3\":\"avalue3\"" +
                       "}}";
            var result = json.FromJson<TestPersistable>(this.factory.Object);

            result.Should().Be(persistable);
            this.factory.Verify(pf => pf.Rehydrate<TestPersistable>(It.Is<PersistableProperties>(props =>
                props.Count == 1
                && (string)((Dictionary<string, object>)props[
                    $"{nameof(TestPersistable.ADictionary)}"])["aname1"] ==
                "avalue1"
                && (string)((Dictionary<string, object>)props[
                    $"{nameof(TestPersistable.ADictionary)}"])["aname2"] ==
                "avalue2"
                && (string)((Dictionary<string, object>)props[
                    $"{nameof(TestPersistable.ADictionary)}"])["aname3"] ==
                "avalue3"
            )));
        }

        [Fact]
        public void WhenFromJsonWithADictionaryOfPersistableInJson_ThenRehydratesDictionary()
        {
            var persistable = new TestPersistable();
            this.factory.Setup(pf =>
                    pf.Rehydrate<TestPersistable>(It.IsAny<PersistableProperties>()))
                .Returns(persistable);

            var json = $"{{\"{nameof(TestPersistable.APersistableDictionary)}\":{{" +
                       $"    \"aname1\":{{\"{nameof(TestPersistable.AProperty)}\":\"avalue1\"}}," +
                       $"    \"aname2\":{{\"{nameof(TestPersistable.AProperty)}\":\"avalue2\"}}," +
                       $"    \"aname3\":{{\"{nameof(TestPersistable.AProperty)}\":\"avalue3\"}}" +
                       "}}";
            var result = json.FromJson<TestPersistable>(this.factory.Object);

            result.Should().Be(persistable);
            this.factory.Verify(pf => pf.Rehydrate<TestPersistable>(It.Is<PersistableProperties>(props =>
                props.Count == 1
                && (string)((Dictionary<string, object>)((Dictionary<string, object>)props[
                    $"{nameof(TestPersistable.APersistableDictionary)}"])["aname1"])[
                    $"{nameof(TestPersistable.AProperty)}"] == "avalue1"
                && (string)((Dictionary<string, object>)((Dictionary<string, object>)props[
                    $"{nameof(TestPersistable.APersistableDictionary)}"])["aname2"])[
                    $"{nameof(TestPersistable.AProperty)}"] == "avalue2"
                && (string)((Dictionary<string, object>)((Dictionary<string, object>)props[
                    $"{nameof(TestPersistable.APersistableDictionary)}"])["aname3"])[
                    $"{nameof(TestPersistable.AProperty)}"] == "avalue3"
            )));
        }
    }
}