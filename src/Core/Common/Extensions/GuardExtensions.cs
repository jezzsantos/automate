using System;
using System.Linq;

namespace Automate.Common.Extensions
{
    public static class GuardExtensions
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
            string parameterName, string errorMessage = null, params object[] args)
        {
            validator.GuardAgainstNull(nameof(validator));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var isValid = validator(value);
            if (!isValid)
            {
                if (errorMessage.HasValue())
                {
                    var messageArgs = args.Exists() && args.Any()
                        ? new object[] { value }.Concat(args).ToArray()
                        : new object[] { value };
                    throw new ArgumentOutOfRangeException(parameterName, value,
                        string.Format(errorMessage ?? string.Empty, messageArgs));
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