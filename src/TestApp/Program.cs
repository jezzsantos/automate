using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading;

namespace TestApp
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var fail = new Option<bool>("--fails", () => false, "Throws an exception");
            var succeed = new Option<bool>("--succeeds", () => true, "Succeeds the application");
            var hang = new Option<bool>("--hangs", () => false, "Hangs the application for a minute");

            var command = new RootCommand
            {
                hang,
                fail,
                succeed
            };
            command.Description = "Test Application";
            command.SetHandler((bool hangs, bool fails, bool succeeds) =>
            {
                if (hangs)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
                if (fails)
                {
                    throw new Exception("Failed");
                }
                if (succeeds)
                {
                    Console.WriteLine("Success");
                }
            }, hang, fail, succeed);

            var parser = new CommandLineBuilder(command)
                .UseDefaults()
                .UseExceptionHandler((ex, context) =>
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(ex.Message);
                    Console.ResetColor();
                    context.ExitCode = 1;
                })
                .Build();

            return parser.Invoke(args);
        }
    }
}