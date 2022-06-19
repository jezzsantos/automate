using System;
using Automate.Extensions;
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

        public AndConstraint<CliTestSetupAssertions> DisplayHelp(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.Output.Value)
                .ForCondition(text => text.StartsWith("Description:") && text.Contains("Usage:"))
                .FailWith(
                    "Expected {context:directory} to display the help banner, but it did not, and instead contained {0}.",
                    Subject.Output.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

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
                .FailWith("Expected {context:output} to contain {0}{reason}, but found {1}.", errorText,
                    Subject.Error.Value);

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
                .FailWith("Expected {context:output} to contain {0}{reason}, but found {1}.", errorMessage,
                    Subject.Error.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayErrorForMissingCommand()
        {
            var errorMessage = "Required command was not provided.";
            Execute.Assertion
                .Given(() => Subject.Error.Value)
                .ForCondition(value => value.Contains(errorMessage))
                .FailWith("Expected {context:output} to contain {0}{reason}, but found {1}.", errorMessage,
                    Subject.Error.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayNoError(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.Error.Value)
                .ForCondition(value => !value.HasValue())
                .FailWith("Expected {context:output} to contain no error {reason}, but found {0}.",
                    Subject.Error.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }

        public AndConstraint<CliTestSetupAssertions> DisplayMessage(string messageText, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(messageText))
                .FailWith("You can't assert a message is displayed without specifying the message")
                .Then
                .Given(() => Subject.Output.Value)
                .ForCondition(value => value.Contains(messageText ?? string.Empty))
                .FailWith("Expected {context:output} to contain {0} {reason}, but found {1}.", messageText,
                    Subject.Output.Value);

            return new AndConstraint<CliTestSetupAssertions>(this);
        }
    }
}