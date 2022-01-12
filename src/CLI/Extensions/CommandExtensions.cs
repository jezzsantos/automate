using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;

namespace automate.Extensions
{
    internal static class CommandExtensions
    {
        public static Command WithHandler(this Command command, string name)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(Program).GetMethod(name, flags);

            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }
    }
}