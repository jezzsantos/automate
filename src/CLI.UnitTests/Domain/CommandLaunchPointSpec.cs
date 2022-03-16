using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using FluentAssertions;
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
                new CommandLaunchPoint("12345678", "alaunchpointname", new List<string> { IdGenerator.Create() });
        }

        [Fact]
        public void WhenConstructedAndNameIsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("12345678", null, new List<string> { "acmdid" }))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndNameIsInvalid_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("12345678", "^aninvalidname^", new List<string> { "acmdid" }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.InvalidNameIdentifier.Format("^aninvalidname^") + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsMissing_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("12345678", "aname", (List<string>)null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructedAndCommandIdsEmpty_ThenThrows()
        {
            FluentActions.Invoking(() => new CommandLaunchPoint("12345678", "aname", new List<string>()))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_EmptyCommandIds + "*");
        }

        [Fact]
        public void WhenConstructedAndCommandIdsInvalid_ThenThrows()
        {
            var cmdIds = new List<string> { IdGenerator.Create(), "aninvalidcmdid", IdGenerator.Create() };
            FluentActions.Invoking(() => new CommandLaunchPoint("12345678", "aname", cmdIds))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(ValidationMessages.Automation_InvalidCommandIds.Format(cmdIds.Join(", ")) +
                             "*");
        }

        [Fact]
        public void WhenExecuteAndCommandNotFound_ThenThrows()
        {
            var solution = new SolutionDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));
            var solutionItem = solution.Model;

            this.launchPoint
                .Invoking(x => x.Execute(solution, solutionItem))
                .Should().Throw<AutomateException>()
                .WithMessage(ExceptionMessages.CommandLaunchPoint_CommandIdNotFound.Format(this.launchPoint.CommandIds.First()));
        }

        [Fact]
        public void WhenExecuteAndCommandOnPattern_ThenExecutesCommandOnSingleElement()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            var pattern = new PatternDefinition("apatternname");
            pattern.AddAutomation(automation);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            var solutionItem = solution.Model;

            var result = new CommandLaunchPoint("12345678", "alaunchpointname", new List<string> { automation.Id })
                .Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().ContainSingle("testingonly");
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void WhenExecuteAndCommandOnDescendantElement_ThenExecutesCommandOnSingleElement()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.AddAutomation(automation);
            element1.AddElement(element2);
            var pattern = new PatternDefinition("apatternname");
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            var solutionItem = solution.Model
                .Properties["anelementname1"].Materialise()
                .Properties["anelementname2"].Materialise();

            var result = new CommandLaunchPoint("12345678", "alaunchpointname", new List<string> { automation.Id })
                .Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().ContainSingle("testingonly");
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void WhenExecuteAndCommandOnDescendantCollection_ThenExecutesCommandOnEachItem()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.AddAutomation(automation);
            element1.AddElement(collection1);
            var pattern = new PatternDefinition("apatternname");
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            var solutionItem = solution.Model
                .Properties["anelementname1"].Materialise()
                .Properties["acollectionname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();

            var result = new CommandLaunchPoint("12345678", "alaunchpointname", new List<string> { automation.Id })
                .Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().Contain("testingonly", "testingonly");
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void WhenExecuteAndAutomationFails_ThenContinuesAndFails()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnly, new Dictionary<string, object> { { "FailTurn", 2 } });
            var element1 = new Element("anelementname1");
            var collection1 = new Element("acollectionname1", isCollection: true);
            collection1.AddAutomation(automation);
            element1.AddElement(collection1);
            var pattern = new PatternDefinition("apatternname");
            pattern.AddElement(element1);
            var solution = new SolutionDefinition(new ToolkitDefinition(pattern));
            var solutionItem = solution.Model
                .Properties["anelementname1"].Materialise()
                .Properties["acollectionname1"].Materialise();
            solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            solution.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();

            var result = new CommandLaunchPoint("12345678", "alaunchpointname", new List<string> { automation.Id })
                .Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().Contain("testingonly", DomainMessages.CommandLaunchPoint_CommandIdFailedExecution.Format(this.launchPoint.CommandIds.First(), "anexceptionmessage"));
            result.IsSuccess.Should().BeFalse();
        }
    }
}