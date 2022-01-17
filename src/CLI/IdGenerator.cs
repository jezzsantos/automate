using System;

namespace automate
{
    internal static class IdGenerator
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}