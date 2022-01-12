using System;

namespace automate.Extensions
{
    internal static class GuardExtensions
    {
        public static void GuardAgainstNull(this object instance, string parameterName = null)
        {
            if (instance == null)
            {
                var ex = parameterName == null
                    ? new ArgumentNullException()
                    : new ArgumentNullException(parameterName);

                throw ex;
            }
        }

        public static void GuardAgainstNullOrEmpty(this string instance, string parameterName = null)
        {
            if (!instance.HasValue())
            {
                var ex = parameterName == null
                    ? new ArgumentNullException()
                    : new ArgumentNullException(parameterName);

                throw ex;
            }
        }

        public static void GuardAgainstInvalid<TValue>(this TValue value, Func<TValue, bool> validator,
            string parameterName, string errorMessage = null)
        {
            validator.GuardAgainstNull(nameof(validator));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var isValid = validator(value);
            if (!isValid)
            {
                if (errorMessage.HasValue())
                {
                    throw new ArgumentOutOfRangeException(parameterName, errorMessage);
                }
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void GuardAgainstMinValue(this DateTime value, string parameterName)
        {
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            if (!value.HasValue())
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}