using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CLI.UnitTests.Domain
{
    [Trait("Category", "Unit")]
    public class CommandLaunchPointSpec
    {
        private readonly CommandLaunchPoint launchPoint;

        public CommandLaunchPointSpec()
        {
            this.launchPoint =
                new CommandLaunchPoint("alaunchpointname", new List<string> { IdGenerator.Create() });
        }

        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint(null, new List<string> { "acmdid" }))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("^aninvalidname^", new List<string> { "acmdid" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndCommandIdsEmpty_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", new List<string>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_EmptyCommandIds + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsInvalid_ThenThrows()
        {
            var cmdIds = new List<string> { IdGenerator.Create(), "aninvalidcmdid", IdGenerator.Create() };
            FluentActions.Invoking(() => new CommandLaunchPoint("aname", cmdIds))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_InvalidCommandIds.Format(cmdIds.Join(", ")) +
                             "*");
        }

        [Fact]
        public void WhenExecuteAndCommandNotFound_ThenThrows()
        {
            var solution = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname"), "1.0"));
            var solutionItem = solution.Model;

            this.launchPoint
                .Invoking(x => x.Execute(solution, solutionItem))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.CommandLaunchPoint_CommandIdNotFound.Format(this.launchPoint.CommandIds.First()));
        }

        [Fact]
        public void WhenExecuteAndCommandOnPattern_ThenExecutesCommandOnSingleElement()
        {
            var automation = new Mock<IAutomation>();
            automation.Setup(auto => auto.Id)
                .Returns(this.launchPoint.CommandIds.First());
            var pattern = new PatternDefinition("apatternname");
            pattern.Automation.Add(automation.Object);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            var solutionItem = solution.Model;
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), solutionItem))
                .Returns(new CommandExecutionResult("anautomationname", new List<string> { "alogentry" }));

            var result = this.launchPoint.Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().ContainSingle("alogentry");
            result.IsSuccess.Should().BeTrue();
            automation.Verify(aut => aut.Execute(solution, solutionItem));
        }

        [Fact]
        public void WhenExecuteAndCommandOnDescendantElement_ThenExecutesCommandOnSingleElement()
        {
            var automation = new Mock<IAutomation>();
            automation.Setup(auto => auto.Id)
                .Returns(this.launchPoint.CommandIds.First());
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.Automation.Add(automation.Object);
            element1.Elements.Add(element2);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            var solutionItem = solution.Model
                .Properties["anelementname1"].Materialise()
                .Properties["anelementname2"].Materialise();
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), solutionItem))
                .Returns(new CommandExecutionResult("anautomationname", new List<string> { "alogentry" }));

            var result = this.launchPoint.Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().ContainSingle("alogentry");
            result.IsSuccess.Should().BeTrue();
            automation.Verify(aut => aut.Execute(solution, solutionItem));
        }

        [Fact]
        public void WhenExecuteAndCommandOnDescendantCollection_ThenExecutesCommandOnEachItem()
        {
            var automation = new Mock<IAutomation>();
            automation.Setup(auto => auto.Id)
                .Returns(this.launchPoint.CommandIds.First());
            var element1 = new Element("anelementname1");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.Automation.Add(automation.Object);
            element1.Elements.Add(collection1);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            var solutionItem = solution.Model
                .Properties["anelementname1"].Materialise()
                .Properties["acollectionname1"].Materialise();
            var collectionItem1 = solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            var collectionItem2 = solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), collectionItem1))
                .Returns(new CommandExecutionResult("anautomationname", new List<string> { "alogentry1" }));
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), collectionItem2))
                .Returns(new CommandExecutionResult("anautomationname", new List<string> { "alogentry2" }));

            var result = this.launchPoint.Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().Contain("alogentry1", "alogentry2");
            result.IsSuccess.Should().BeTrue();
            automation.Verify(aut => aut.Execute(solution, collectionItem1));
            automation.Verify(aut => aut.Execute(solution, collectionItem2));
        }

        [Fact]
        public void WhenExecuteAndCommandFails_ThenContinuesAndFails()
        {
            var automation = new Mock<IAutomation>();
            automation.Setup(auto => auto.Id)
                .Returns(this.launchPoint.CommandIds.First());
            var element1 = new Element("anelementname1");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.Automation.Add(automation.Object);
            element1.Elements.Add(collection1);
            var pattern = new PatternDefinition("apatternname");
            pattern.Elements.Add(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern, "1.0"));
            var solutionItem = solution.Model
                .Properties["anelementname1"].Materialise()
                .Properties["acollectionname1"].Materialise();
            var collectionItem1 = solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            var collectionItem2 = solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), collectionItem1))
                .Returns(new CommandExecutionResult("anautomationname", new List<string> { "alogentry1" }));
            automation.Setup(auto => auto.Execute(It.IsAny<SolutionDefinition>(), collectionItem2))
                .Throws(new Exception("anexceptionmessage"));

            var result = this.launchPoint.Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().Contain("alogentry1", DomainMessages.CommandLaunchPoint_CommandIdFailedExecution.Format(this.launchPoint.CommandIds.First(), "anexceptionmessage"));
            result.IsSuccess.Should().BeFalse();
            automation.Verify(aut => aut.Execute(solution, collectionItem1));
            automation.Verify(aut => aut.Execute(solution, collectionItem2));
        }
    }
}