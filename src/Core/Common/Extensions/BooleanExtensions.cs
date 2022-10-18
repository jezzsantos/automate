namespace Automate.Common.Extensions
{
    public static class BooleanExtensions
    {
        public static bool ToBool(this object value, bool? defaultValue = false)
        {
            if (!bool.TryParse(value?.ToString(), out var result))
            {
                return defaultValue ?? false;
            }

            return result;
        }
    }
}