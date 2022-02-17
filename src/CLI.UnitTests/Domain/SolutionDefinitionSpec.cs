using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class SolutionDefinitionSpec
    {
        [Fact]
        public void WhenConstructed_ThenInitialisesSchema()
        {
            var pattern = new PatternDefinition("apatternname");
            var element3 = new Element("anelementname3", "adisplayname3", "adescription3", true);
            element3.Attributes.Add(new Attribute("anattributename3", "string", false, "adefaultvalue3"));
            var element2 = new Element("anelementname2", "adisplayname2", "adescription2", true);
            element2.Attributes.Add(new Attribute("anattributename2", "string", false, "adefaultvalue2"));
            var element1 = new Element("anelementname1", "adisplayname1", "adescription1", true);
            element1.Attributes.Add(new Attribute("anattributename1", "string", false, "adefaultvalue1"));

            pattern.Elements.Add(element1);
            pattern.Elements.Add(element2);
            pattern.Elements.Add(element3);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));

            solution.Model.Should().NotBeNull();
            var solutionElement1 = solution.Model.Properties["anelementname1"];
            solutionElement1.ElementSchema.Should().Be(element1);
            solutionElement1.Value.Should().BeNull();
            solutionElement1.IsMaterialised.Should().BeFalse();
            solutionElement1.Items.Should().BeNull();

            var solutionElement2 = solution.Model.Properties["anelementname2"];
            solutionElement2.ElementSchema.Should().Be(element2);
            solutionElement2.Value.Should().BeNull();
            solutionElement2.IsMaterialised.Should().BeFalse();
            solutionElement2.Items.Should().BeNull();

            var solutionElement3 = solution.Model.Properties["anelementname3"];
            solutionElement3.ElementSchema.Should().Be(element3);
            solutionElement3.Value.Should().BeNull();
            solutionElement3.IsMaterialised.Should().BeFalse();
            solutionElement3.Items.Should().BeNull();
        }

        [Fact]
        public void WhenGetConfiguration_ThenReturnsConfiguration()
        {
            var attribute1 = new Attribute("anattributename1", null, false, "adefaultvalue1");
            var attribute2 = new Attribute("anattributename2", null, false, "adefaultvalue2");
            var attribute3 = new Attribute("anattributename3", "int", false, "25");
            var elementLevel1 = new Element("anelementname1", "adisplayname1", "adescription1");
            var elementLevel2 = new Element("anelementname2", "adisplayname2", "adescription2");
            var collectionLevel1 = new Element("acollectionname2", "adisplayname1", "adescription1", true);
            elementLevel2.Attributes.Add(attribute2);
            collectionLevel1.Attributes.Add(attribute3);
            elementLevel1.Elements.Add(elementLevel2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Attributes.Add(attribute1);
            pattern.Elements.Add(elementLevel1);
            pattern.Elements.Add(collectionLevel1);

            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            solution.Model.Properties["anelementname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["anelementname2"].Materialise();
            solution.Model.Properties["acollectionname2"].MaterialiseCollectionItem();

            var result = solution.GetConfiguration();

            result.Should().Be(new
            {
                anattributename1 = "adefaultvalue1",
                anelementname1 = new
                {
                    anelementname2 = new
                    {
                        anattributename2 = "adefaultvalue2"
                    }
                },
                acollectionname2 = new
                {
                    items = new[]
                    {
                        new
                        {
                            anattributename3 = 25
                        }
                    }
                }
            }.ToJson<dynamic>());
        }
    }
}