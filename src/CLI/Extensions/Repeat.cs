using System;
using System.Linq;

namespace Automate.CLI.Extensions
{
    internal static class Repeat
    {
        public static void Times(Action action, int count)
        {
            Times(action, 0, count);
        }

        public static void Times(Action<int> action, int count)
        {
            Times(action, 0, count);
        }

        private static void Times(Action action, int from, int to)
        {
            var counter = Enumerable.Range(from, to).ToList();
            counter.ForEach(index => { action(); });
        }

        private static void Times(Action<int> action, int from, int to)
        {
            var counter = Enumerable.Range(from, to).ToList();
            counter.ForEach(index => { action(index + 1); });
        }
    }
}