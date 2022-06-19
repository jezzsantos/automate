using System;
using System.Diagnostics;
using Automate.Common.Extensions;
using Automate.Runtime.Application;

namespace Automate.CLI.Infrastructure
{
    public class ApplicationExecutor : IApplicationExecutor
    {
        internal static readonly TimeSpan HangTime = TimeSpan.FromSeconds(5);

        public ApplicationExecutionProcessResult RunApplicationProcess(bool awaitForExit, string applicationName,
            string arguments)
        {
            applicationName.GuardAgainstNullOrEmpty(nameof(applicationName));

            var outcome = new ApplicationExecutionProcessResult();

            try
            {
                var appName = Environment.ExpandEnvironmentVariables(applicationName);
                var args = arguments.HasValue()
                    ? Environment.ExpandEnvironmentVariables(arguments)
                    : arguments;
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = appName,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Environment.CurrentDirectory
                });
                if (process.NotExists())
                {
                    throw new InvalidOperationException();
                }
                if (process.HasExited)
                {
                    var error = process.StandardError.ReadToEnd();
                    if (error.HasValue())
                    {
                        throw new Exception(error);
                    }

                    throw new Exception(
                        InfrastructureMessages.ApplicationExecutor_ProcessExited.Substitute(process.ExitCode));
                }

                if (!awaitForExit)
                {
                    outcome.Succeeds(
                        InfrastructureMessages.ApplicationExecutor_Succeeded_NotAwaited.Substitute(applicationName,
                            arguments));
                }
                else
                {
                    var success = process.WaitForExit((int)HangTime.TotalMilliseconds);
                    if (success)
                    {
                        var error = process.StandardError.ReadToEnd();
                        if (error.HasValue())
                        {
                            throw new Exception(error);
                        }

                        var output = process.StandardOutput.ReadToEnd();
                        outcome.Succeeds(
                            InfrastructureMessages.ApplicationExecutor_Succeeded.Substitute(applicationName, arguments,
                                output));
                    }
                    else
                    {
                        process.Kill();
                        outcome.Fails(InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Substitute(
                            applicationName,
                            arguments,
                            InfrastructureMessages.ApplicationExecutor_Hung.Substitute(HangTime.TotalSeconds)));
                    }
                }
            }
            catch (Exception ex)
            {
                outcome.Fails(
                    InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Substitute(applicationName, arguments,
                        ex.Message));
            }

            return outcome;
        }
    }
}