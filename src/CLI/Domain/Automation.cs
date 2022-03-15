using System;
using System.Collections.Generic;
using Automate.CLI.Extensions;
using ServiceStack;

namespace Automate.CLI.Domain
{
    internal enum AutomationType
    {
        Unknown = 0,
        CodeTemplateCommand = 1,
        CommandLaunchPoint = 2,
        TestingOnly = 100
    }

    internal class Automation : IPersistable
    {
        private static int testTurn;

        public Automation(string name, AutomationType type, Dictionary<string, object> metadata)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            metadata.GuardAgainstNull(nameof(metadata));

            Id = IdGenerator.Create();
            Name = name;
            Type = type;
            Metadata = metadata;
            testTurn = 0;
        }

        private Automation(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Name = properties.Rehydrate<string>(factory, nameof(Name));
            Type = properties.Rehydrate<AutomationType>(factory, nameof(Type));
            Metadata = properties.Rehydrate<Dictionary<string, object>>(factory, nameof(Metadata));
        }

        public AutomationType Type { get; }

        public IReadOnlyDictionary<string, object> Metadata { get; }

        public string Name { get; }

        public string Id { get; }

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Name), Name);
            properties.Dehydrate(nameof(Type), Type);
            properties.Dehydrate(nameof(Metadata), Metadata);

            return properties;
        }

        public static Automation Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new Automation(properties, factory);
        }

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            switch (Type)
            {
                case AutomationType.CodeTemplateCommand:
                {
                    var automation = new CodeTemplateCommand(Id, Name, Metadata); //Challenge here is to give it all the data it needs, including its ID, and its dependencies
                    return automation.Execute(solution, target);
                }

                case AutomationType.CommandLaunchPoint:
                {
                    var automation = new CommandLaunchPoint(Id, Name, Metadata); //Challenge here is to give it all the data it needs, including its ID, and its dependencies
                    return automation.Execute(solution, target);
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
    }
}