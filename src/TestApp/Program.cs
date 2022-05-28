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
            var open = new Option<string>("--opens", "Opens a given file path");

            var command = new RootCommand
            {
                hang,
                fail,
                succeed,
                open
            };
            command.Description = "Test Application";
            command.SetHandler((bool hangs, bool fails, bool succeeds, string opens) =>
            {
                if (hangs)
                {
                    Console.WriteLine("Hanging");
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
                else if (fails)
                {
                    throw new Exception("Failed");
                }
                else if (succeeds)
                {
                    Console.WriteLine("Success");
                }
                else if (!string.IsNullOrEmpty(opens))
                {
                    Console.WriteLine($"Opening: {opens}");
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    Console.WriteLine($"Opened: {opens}");
                }
            }, hang, fail, succeed, open);

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