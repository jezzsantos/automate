using System;
using System.Collections.Generic;
using Automate.Common.Application;
using Automate.Common.Domain;
using Automate.Common.Extensions;
using Automate.Runtime.Domain;

namespace Automate.Authoring.Domain
{
    public class Automation : IAutomation, IPersistable, IPatternVisitable
    {
#if TESTINGONLY
        private static int testTurn;
#endif
        private readonly Dictionary<string, object> metadata;

        public Automation(string name, AutomationType type,
            Dictionary<string, object> metadata)
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
#if TESTINGONLY
            testTurn = 0;
#endif
        }

        private Automation(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Type = properties.Rehydrate<AutomationType>(factory, nameof(Type));
            this.metadata = properties.Rehydrate<Dictionary<string, object>>(factory, nameof(Metadata));
        }

        public IReadOnlyDictionary<string, object> Metadata => this.metadata;

        private PatternElement Parent { get; set; }

        public bool IsLaunching =>
#if TESTINGONLY
            Type == AutomationType.TestingOnlyLaunching ||
#endif

            Type == AutomationType.CommandLaunchPoint;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(Type), Type);
            properties.Dehydrate(nameof(Metadata), this.metadata);

            return properties;
        }

        public static Automation Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new Automation(properties, factory);
        }

        public IAutomation GetExecutable(DraftDefinition draft, DraftItem target)
        {
            switch (Type)
            {
                case AutomationType.CodeTemplateCommand:
                {
                    return CodeTemplateCommand.FromAutomation(this);
                }

                case AutomationType.CliCommand:
                {
                    return CliCommand.FromAutomation(this);
                }

                case AutomationType.CommandLaunchPoint:
                {
                    return CommandLaunchPoint.FromAutomation(this);
                }
#if TESTINGONLY

                case AutomationType.TestingOnlyLaunching:
                case AutomationType.TestingOnlyLaunchable:
                {
                    testTurn++;
                    if (Metadata.ContainsKey("FailTurn"))
                    {
                        if (Metadata["FailTurn"].ToString().ToInt() == testTurn)
                        {
                            throw new Exception("anexceptionmessage");
                        }
                    }
                    return new TestingOnlyAutomation(Name, Type);
                }
#endif

                case AutomationType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(
                        ApplicationMessages.Automation_UnknownAutomationType.Substitute(Type));
            }
        }

        public void SetParent(PatternElement parent)
        {
            Parent = parent;
        }

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

        public AutomationType Type { get; }

        public string Name { get; private set; }

        public string Id { get; }

        public bool IsLaunchable => Type is AutomationType.CliCommand
#if TESTINGONLY
            or AutomationType.TestingOnlyLaunchable
#endif
            or AutomationType.CodeTemplateCommand;

        public bool Accept(IPatternVisitor visitor)
        {
            return visitor.VisitAutomation(this);
        }
    }
}