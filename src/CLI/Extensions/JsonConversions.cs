using ServiceStack.Text;

namespace Automate.CLI.Extensions
{
    internal static class JsonConversions
    {
        public static string ToJson<T>(this T instance) where T : new()
        {
            using (var scope = JsConfig.BeginScope())
            {
                scope.Indent = true;
                return ServiceStack.StringExtensions.ToJson(instance);
            }
        }

        public static T FromJson<T>(this string json) where T : new()
        {
            using (JsConfig.BeginScope())
            {
                return ServiceStack.StringExtensions.FromJson<T>(json);
            }
        }
    }
}