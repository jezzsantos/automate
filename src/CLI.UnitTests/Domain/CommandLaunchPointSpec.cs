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
                new CommandLaunchPoint("alaunchpointname", new List<string> { IdGenerator.Create() });
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

            var result = new CommandLaunchPoint("alaunchpointname", new List<string> { automation.Id })
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

            var result = new CommandLaunchPoint("alaunchpointname", new List<string> { automation.Id })
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

            var result = new CommandLaunchPoint("alaunchpointname", new List<string> { automation.Id })
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

            var result = new CommandLaunchPoint("alaunchpointname", new List<string> { automation.Id })
                .Execute(solution, solutionItem);

            result.CommandName.Should().Be("alaunchpointname");
            result.Log.Should().Contain("testingonly", DomainMessages.CommandLaunchPoint_CommandIdFailedExecution.Format(this.launchPoint.CommandIds.First(), "anexceptionmessage"));
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void WhenAppendCommandIdsAndHasNone_ThenAddsOne()
        {
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid" });

            this.launchPoint.CommandIds.Should().ContainInOrder(this.launchPoint.CommandIds.First(), "acmdid");
        }

        [Fact]
        public void WhenAppendCommandIdsWithNew_ThenAddsNew()
        {
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid1" });

            this.launchPoint.AppendCommandIds(new List<string> { "acmdid2" });

            this.launchPoint.CommandIds.Should().ContainInOrder(this.launchPoint.CommandIds.First(), "acmdid1", "acmdid2");
        }

        [Fact]
        public void WhenAppendCommandIdsWithNewAndDuplicates_ThenAddsNew()
        {
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid1" });
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid2" });
            this.launchPoint.AppendCommandIds(new List<string> { "acmdid3" });

            this.launchPoint.AppendCommandIds(new List<string> { "acmdid2", "acmdid3", "acmdid4" });

            this.launchPoint.CommandIds.Should().ContainInOrder(this.launchPoint.CommandIds.First(), "acmdid1", "acmdid2", "acmdid3", "acmdid4");
        }
    }
}