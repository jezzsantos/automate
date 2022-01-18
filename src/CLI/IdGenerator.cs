using System;

namespace automate
{
    internal static class IdGenerator
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static bool IsValid(string id)
        {
            return Guid.TryParseExact(id, "N", out var _);
        }
    }
}