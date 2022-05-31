using System.Collections;
using Automate.CLI.Domain;
using Automate.CLI.Extensions;
using Humanizer;
using Scriban.Runtime;

namespace Automate.CLI.Infrastructure
{
    internal class TextTemplatingEngine : ITextTemplatingEngine
    {
        public string Transform(string description, string textTemplate, SolutionItem solutionItem)
        {
            solutionItem.GuardAgainstNull(nameof(solutionItem));
            description.GuardAgainstNullOrEmpty(nameof(description));

            var configuration = solutionItem.GetConfiguration(true);

            return configuration.Transform(description, textTemplate, true);
        }

        public string Transform(string description, string textTemplate, IDictionary values)
        {
            values.GuardAgainstNull(nameof(values));
            description.GuardAgainstNullOrEmpty(nameof(description));

            return values.Transform(description, textTemplate, true);
        }
    }

    internal class CustomScribanStringFunctions : ScriptObject
    {
        /// <summary>
        ///     Converts the string to camel case.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The camel cased input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWord" | string.camelcase }}
        ///     ```
        ///     ```html
        ///     testWord
        ///     ```
        /// </remarks>
        public static string Camelcase(string text)
        {
            return text?.ToCamelCase();
        }

        /// <summary>
        ///     Converts the string to pascal case.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The pascal cased input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "testWord" | string.pascalcase }}
        ///     ```
        ///     ```html
        ///     TestWord
        ///     ```
        /// </remarks>
        public static string Pascalcase(string text)
        {
            return text?.ToPascalCase();
        }

        /// <summary>
        ///     Converts the string to plural form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The singular input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWord" | string.to_plural }}
        ///     ```
        ///     ```html
        ///     TestWords
        ///     ```
        /// </remarks>
        public static string ToPlural(string text)
        {
            return text?.Pluralize();
        }

        /// <summary>
        ///     Converts the string to singular form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The plural input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWords" | string.to_singular }}
        ///     ```
        ///     ```html
        ///     TestWord
        ///     ```
        /// </remarks>
        public static string ToSingular(string text)
        {
            return text?.Singularize();
        }
    }
}