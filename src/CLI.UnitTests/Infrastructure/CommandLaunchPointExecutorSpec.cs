﻿using System.Collections.Generic;
using System.Linq;
using Automate.Authoring.Domain;
using Automate.CLI;
using Automate.CLI.Infrastructure;
using Automate.Common;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;
using FluentAssertions;
using Xunit;

namespace CLI.UnitTests.Infrastructure
{
    [Trait("Category", "Unit")]
    public class CommandLaunchPointExecutorSpec
    {
        private readonly CommandLaunchPointExecutor executor;
        private readonly CommandLaunchPoint launchPoint;
        private readonly PatternDefinition pattern;

        public CommandLaunchPointExecutorSpec()
        {
            this.pattern = new PatternDefinition("apatternname");
            this.launchPoint = new CommandLaunchPoint("alaunchpointname",
                new List<string> { IdGenerator.Create() });
            this.executor = new CommandLaunchPointExecutor();
            this.pattern.AddAutomation(this.launchPoint.AsAutomation());
        }

        [Fact]
        public void WhenExecuteAndCommandNotFound_ThenThrows()
        {
            var draft = new DraftDefinition(new ToolkitDefinition(new PatternDefinition("apatternname")));
            var draftItem = draft.Model;
            var executionResult = new CommandExecutionResult("acommandname",
                new CommandExecutableContext(this.launchPoint, draft, draftItem));

            this.executor
                .Invoking(x => x.Execute(this.launchPoint, executionResult, (_, _, _) => { }))
                .Should().Throw<AutomateException>()
                .WithMessage(
                    ExceptionMessages.CommandLaunchPoint_CommandIdNotFound.Substitute(this.launchPoint.CommandIds
                        .First()));
        }
#if TESTINGONLY

        [Fact]
        public void WhenExecuteAndCommandOnPattern_ThenExecutesCommandOnOnlyOneElement()
        {
            var automation =
                new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            this.pattern.AddAutomation(automation);
            var draft = new DraftDefinition(new ToolkitDefinition(this.pattern));
            var draftItem = draft.Model;
            var executionResult = new CommandExecutionResult("alaunchpointname",
                new CommandExecutableContext(this.launchPoint, draft, draftItem));

            new CommandLaunchPointExecutor()
                .Execute(new CommandLaunchPoint("alaunchpointname",
                        new List<string> { automation.Id }), executionResult,
                    (result, _, _) => { result.RecordSuccess("alog"); });

            executionResult.CommandName.Should().Be("alaunchpointname");
            executionResult.Log.Should().ContainSingle("alog");
            executionResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void WhenExecuteAndCommandOnDescendantElement_ThenExecutesCommandOnOnlyOneElement()
        {
            var automation =
                new Automation("alaunchpointname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            var element2 = new Element("anelementname2");
            element2.AddAutomation(automation);
            element1.AddElement(element2);
            this.pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(this.pattern));
            var draftItem = draft.Model
                .Properties["anelementname1"].Materialise()
                .Properties["anelementname2"].Materialise();
            var executionResult = new CommandExecutionResult("alaunchpointname",
                new CommandExecutableContext(this.launchPoint, draft, draftItem));

            new CommandLaunchPointExecutor()
                .Execute(new CommandLaunchPoint("alaunchpointname",
                        new List<string> { automation.Id }), executionResult,
                    (result, _, _) => { result.RecordSuccess("alog"); });

            executionResult.CommandName.Should().Be("alaunchpointname");
            executionResult.Log.Should().ContainSingle("alog");
            executionResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void WhenExecuteAndCommandOnDescendantCollection_ThenExecutesCommandOnEachItem()
        {
            var automation =
                new Automation("alaunchpointname", AutomationType.TestingOnlyLaunchable,
                    new Dictionary<string, object>());
            var element1 = new Element("anelementname1");
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany);
            collection1.AddAutomation(automation);
            element1.AddElement(collection1);
            this.pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(this.pattern));
            var draftItem = draft.Model
                .Properties["anelementname1"].Materialise()
                .Properties["acollectionname1"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            var executionResult = new CommandExecutionResult("alaunchpointname",
                new CommandExecutableContext(this.launchPoint, draft, draftItem));

            new CommandLaunchPointExecutor()
                .Execute(new CommandLaunchPoint("alaunchpointname",
                        new List<string> { automation.Id }), executionResult,
                    (result, _, _) => { result.RecordSuccess("alog"); });

            executionResult.CommandName.Should().Be("alaunchpointname");
            executionResult.Log.Should()
                .Contain(x => x.Type == CommandExecutionLogItemType.Succeeded && x.Message == "alog")
                .And.Contain(x => x.Type == CommandExecutionLogItemType.Succeeded && x.Message == "alog");
            executionResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void WhenExecuteAndAutomationFails_ThenContinuesAndFails()
        {
            var automation = new Automation("acommandname", AutomationType.TestingOnlyLaunchable,
                new Dictionary<string, object> { { "FailTurn", 2 } });
            var element1 = new Element("anelementname1");
            var collection1 = new Element("acollectionname1",
                ElementCardinality.OneOrMany);
            collection1.AddAutomation(automation);
            element1.AddElement(collection1);
            this.pattern.AddElement(element1);
            var draft = new DraftDefinition(new ToolkitDefinition(this.pattern));
            var draftItem = draft.Model
                .Properties["anelementname1"].Materialise()
                .Properties["acollectionname1"].Materialise();
            draft.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            draft.Model.Properties["anelementname1"].Properties["acollectionname1"].MaterialiseCollectionItem();
            var executionResult = new CommandExecutionResult("alaunchpointname",
                new CommandExecutableContext(this.launchPoint, draft, draftItem));

            new CommandLaunchPointExecutor()
                .Execute(new CommandLaunchPoint("alaunchpointname",
                        new List<string> { automation.Id }), executionResult,
                    (result, _, _) => { result.RecordSuccess("alog"); });

            executionResult.CommandName.Should().Be("alaunchpointname");
            executionResult.Log.Should()
                .Contain(x => x.Type == CommandExecutionLogItemType.Succeeded && x.Message == "alog")
                .And.Contain(x => x.Type == CommandExecutionLogItemType.Failed && x.Message ==
                    InfrastructureMessages.CommandLaunchPoint_CommandIdFailedExecution.Substitute(
                        automation.Id, "anexceptionmessage"));
            executionResult.IsSuccess.Should().BeFalse();
        }
#endif
    }
}