using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;

namespace Automate.CLI.Extensions
{
    internal static class CommandExtensions
    {
        public static Command WithHandler<THandlers>(this Command command, string name)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(THandlers).GetMethod(name, flags);

            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }

        public static Command AsHidden(this Command command)
        {
            command.IsHidden = true;

            return command;
        }

        public static Command WithAlias(this Command command, string alias)
        {
            command.AddAlias(alias);

            return command;
        }

        public static Option WithAlias(this Option option, string alias)
        {
            option.AddAlias(alias);

            return option;
        }
    }
}