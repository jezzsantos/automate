﻿using System;
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
        private readonly Dictionary<string, object> metadata;

        public Automation(string name, AutomationType type, Dictionary<string, object> metadata)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            name.GuardAgainstInvalid(Validations.IsNameIdentifier, nameof(name),
                ValidationMessages.InvalidNameIdentifier);
            metadata.GuardAgainstNull(nameof(metadata));

            Id = IdGenerator.Create();
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

        public string Name { get; }

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

        public CommandExecutionResult Execute(SolutionDefinition solution, SolutionItem target)
        {
            switch (Type)
            {
                case AutomationType.CodeTemplateCommand:
                {
                    var automation = CodeTemplateCommand.FromAutomation(this);
                    return automation.Execute(solution, target);
                }

                case AutomationType.CommandLaunchPoint:
                {
                    var automation = CommandLaunchPoint.FromAutomation(this);
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

        public void UpdateMetadata(string name, object value)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            value.GuardAgainstNull(nameof(value));
            this.metadata[name] = value;
        }
    }
}