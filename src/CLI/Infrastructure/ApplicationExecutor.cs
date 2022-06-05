using System;
using System.Diagnostics;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;

namespace Automate.CLI.Infrastructure
{
    internal class ApplicationExecutor : IApplicationExecutor
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
                        InfrastructureMessages.ApplicationExecutor_ProcessExited.Format(process.ExitCode));
                }

                if (!awaitForExit)
                {
                    outcome.Succeeds(
                        InfrastructureMessages.ApplicationExecutor_Succeeded_NotAwaited.Format(applicationName,
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
                            InfrastructureMessages.ApplicationExecutor_Succeeded.Format(applicationName, arguments,
                                output));
                    }
                    else
                    {
                        process.Kill();
                        outcome.Fails(InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Format(applicationName,
                            arguments,
                            InfrastructureMessages.ApplicationExecutor_Hung.Format(HangTime.TotalSeconds)));
                    }
                }
            }
            catch (Exception ex)
            {
                outcome.Fails(
                    InfrastructureMessages.ApplicationExecutor_ExecutionFailed.Format(applicationName, arguments,
                        ex.Message));
            }

            return outcome;
        }
    }
}