using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal enum AutomationType
    {
        Unknown = 0,
        CodeTemplateCommand = 1,
        CliCommand = 2,
        CommandLaunchPoint = 10,
        TestingOnly = 100
    }

    internal class Automation : IPersistable, IPatternVisitable
    {
        private static int testTurn;
        private readonly Dictionary<string, object> metadata;

        public Automation(string name, AutomationType type, Dictionary<string, object> metadata)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            metadata.GuardAgainstNull(nameof(metadata));

            Id = IdGenerator.Create();
            Parent = null;
            Name = name;
            Type = type;
            this.metadata = metadata;
            testTurn = 0;
        }

        private Automation(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Type = properties.Rehydrate<AutomationType>(factory, nameof(Type));
            this.metadata = properties.Rehydrate<Dictionary<string, object>>(factory, nameof(Metadata));
        }

        public AutomationType Type { get; }

        public IReadOnlyDictionary<string, object> Metadata => this.metadata;

        public string Name { get; private set; }

        public string Id { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(Type), Type);
            properties.Dehydrate(nameof(Metadata), this.metadata);

            return properties;
        }

        public static Automation Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new Automation(properties, factory);
        }

        public CommandExecutionResult Execute(DraftDefinition draft, DraftItem target)
        {
            switch (Type)
            {
                case AutomationType.CodeTemplateCommand:
                {
                    var automation = CodeTemplateCommand.FromAutomation(this);
                    return automation.Execute(draft, target);
                }

                case AutomationType.CliCommand:
                {
                    var automation = CliCommand.FromAutomation(this);
                    return automation.Execute(draft, target);
                }

                case AutomationType.CommandLaunchPoint:
                {
                    var automation = CommandLaunchPoint.FromAutomation(this);
                    return automation.Execute(draft, target);
                }

                case AutomationType.TestingOnly:
                {
                    testTurn++;
                    if (Metadata.ContainsKey("FailTurn"))
                    {
                        if (Metadata["FailTurn"].ToString().ToInt() == testTurn)
                        {
                            throw new Exception("anexceptionmessage");
                        }
                    }
                    return new CommandExecutionResult(Name, new List<string> { "testingonly" });
                }

                default:
                    throw new ArgumentOutOfRangeException($"Unknown type of automation: {Type}");
            }
        }

        public void SetParent(PatternElement parent)
        {
            Parent = parent;
        }

        private PatternElement Parent { get; set; }

        public void Rename(string name)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);

            if (name.NotEqualsOrdinal(Name))
            {
                Name = name;
                Parent.RecordChange(VersionChange.NonBreaking, VersionChanges.Automation_Update_Name, Id, Parent.Id);
            }
        }

        public void UpdateMetadata(string name, object value)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            value.GuardAgainstNull(nameof(value));

            if (!this.metadata.ContainsKey(name)
                || this.metadata[name] != value)
            {
                this.metadata[name] = value;
                Parent.RecordChange(VersionChange.NonBreaking, VersionChanges.Automation_Update_Metadata, Id,
                    Parent.Id);
            }
        }

        public bool Accept(IPatternVisitor visitor)
        {
            return visitor.VisitAutomation(this);
        }
    }
}