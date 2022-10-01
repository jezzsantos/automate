using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Automate.Common.Extensions;
using JetBrains.Annotations;

namespace Automate.CLI.Infrastructure
{
    internal partial class CommandLineApi
    {
#if TESTINGONLY
        [UsedImplicitly]
        internal class TestingOnlyApiHandlers : HandlerBase
        {
            [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
            [SuppressMessage("ReSharper", "UnusedParameter.Global")]
            internal static void Fail(string message, bool nested)
            {
                Output(OutputMessages.CommandLine_Output_TestingOnly, "avalue");
                Output(OutputMessages.CommandLine_Output_TestingOnly, JsonNode.Parse(new
                {
                    AProperty1 = new
                    {
                        AChildProperty1 = "avalue1"
                    },
                    AProperty2 = "avalue2"
                }.ToJson()));
                if (nested)
                {
                    throw new Exception(message, new Exception(message));
                }
                throw new Exception(message);
            }

            internal static void Succeed(string message, string value)
            {
                Output(message, value);
                Output(OutputMessages.CommandLine_Output_TestingOnly, JsonNode.Parse(new
                {
                    AProperty1 = new
                    {
                        AChildProperty1 = "avalue1"
                    },
                    AProperty2 = "avalue2"
                }.ToJson()));
            }
        }
#endif
    }
}