using System;

namespace Automate.Common.Extensions
{
    public static class Try
    {
        public static TReturn Safely<TReturn>(Func<TReturn> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                if (ex is StackOverflowException
                    || ex is OutOfMemoryException)
                {
                    throw;
                }

                //Ignore exception!
                return default;
            }
        }
    }
}