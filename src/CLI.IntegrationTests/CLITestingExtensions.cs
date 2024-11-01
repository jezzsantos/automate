﻿using System;
using Automate.Common.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace CLI.IntegrationTests
{
    internal static class CliTestingExtensions
    {
        public static CliTestSetupAssertions Should(this CliTestSetup instance)
        {
            return new CliTestSetupAssertions(instance);
        }
    }

    internal class CliTestSetupAssertions : ReferenceTypeAssertions<CliTestSetup, CliTestSetupAssertions>
    {
        public CliTestSetupAssertions(CliTestSetup instance) : base(instance)
        {
        }

        protected override string Identifier => "output";

        public AndConstraint<CliTestSetupAssertions> DisplayError(string errorText, params object[] errorArgs)
        {
            var errorMessage = errorText.Substitute(errorArgs);
            Execute.Assertion
                .ForCondition(!string.IsNullOrEmpty(errorText))
                .FailWith("You can't assert an error is displayed without specifying the text of the error")
                .Then
                .Given(() => Subject.Error.Value)
                .ForCondition(value =>
                    value.Trim(Environment.NewLine.ToCharArray()) == errorMessage || value.Contains(errorMessage))
                .FailWith("Expected {context:StdError} to contain {0}{reason}, but found {1}.", errorText,
                    Subject.Error.Value);

            Execute.Assertion
                .Given(() => Subject.ExitCode)
                .ForCondition(value => value == 1)
                .FailWith("Expected {context:ExitCode} to be 1{reason}, but found {0}.", Subject.ExitCode);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayWarning(string errorText, params object[] errorArgs)
        {
            var errorMessage = errorText.Substitute(errorArgs);
            Execute.Assertion
                .ForCondition(!string.IsNullOrEmpty(errorText))
                .FailWith("You can't assert an error is displayed without specifying the text of the error")
                .Then
                .Given(() => Subject.Value.Value)
                .ForCondition(value =>
                    value.Trim(Environment.NewLine.ToCharArray()) == errorMessage || value.Contains(errorMessage))
                .FailWith("Expected {context:StdOutput} to contain {0}{reason}, but found {1}.", errorText,
                    Subject.Value.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayErrorForMissingArgument(string argumentName)
        {
            var errorMessage = $"Required argument missing for command: '{argumentName}'.";
            Execute.Assertion
                .ForCondition(!string.IsNullOrEmpty(argumentName))
                .FailWith("You can't assert an error is displayed without specifying the argument of the error")
                .Then
                .Given(() => Subject.Error.Value)
                .ForCondition(value => value.Contains(errorMessage))
                .FailWith("Expected {context:StdError} to contain {0}{reason}, but found {1}.", errorMessage,
                    Subject.Error.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayErrorForMissingCommand()
        {
            var errorMessage = "Required command was not provided.";
            Execute.Assertion
                .Given(() => Subject.Error.Value)
                .ForCondition(value => value.Contains(errorMessage))
                .FailWith("Expected {context:StdError} to contain {0}{reason}, but found {1}.", errorMessage,
                    Subject.Error.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayNoError(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.Error.Value)
                .ForCondition(value => !value.HasValue())
                .FailWith("Expected {context:StdError} to contain no text{reason}, but found {0}.",
                    Subject.Error.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayOutput(string messageText, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(messageText))
                .FailWith("You can't assert a message is displayed without specifying the message")
                .Then
                .Given(() => Subject.Value.Value)
                .ForCondition(value => value.Contains(messageText ?? string.Empty))
                .FailWith("Expected {context:StdOutput} to contain {0} {reason}, but found {1}.", messageText,
                    Subject.Value.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayNoOutput(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.Value.Value)
                .ForCondition(value => value.Equals(Environment.NewLine))
                .FailWith("Expected {context:StdOutput} to contain no text{reason}, but found {0}.",
                    Subject.Value.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }
    }
}