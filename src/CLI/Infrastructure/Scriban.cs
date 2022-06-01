using Automate.CLI.Extensions;
using Humanizer;
using Scriban.Runtime;

namespace Automate.CLI.Infrastructure
{
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
        ///     Converts the string to snake case.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The snake cased input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "testWord" | string.snakecase }}
        ///     ```
        ///     ```html
        ///     test_word
        ///     ```
        /// </remarks>
        public static string Snakecase(string text)
        {
            return text?.ToSnakeCase();
        }

        /// <summary>
        ///     Converts the string to its plural form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The singular input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWord" | string.pluralize }}
        ///     ```
        ///     ```html
        ///     TestWords
        ///     ```
        /// </remarks>
        public static string Pluralize(string text)
        {
            return text?.Pluralize();
        }

        /// <summary>
        ///     Converts the string to its singular form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The plural input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWords" | string.singularize }}
        ///     ```
        ///     ```html
        ///     TestWord
        ///     ```
        /// </remarks>
        public static string Singularize(string text)
        {
            return text?.Singularize();
        }

        /// <summary>
        ///     Converts the string to its pascal cased plural form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The singular input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWord" | string.pascalplural }}
        ///     ```
        ///     ```html
        ///     TestWords
        ///     ```
        /// </remarks>
        public static string Pascalplural(string text)
        {
            return text?.ToPascalCase().Pluralize();
        }

        /// <summary>
        ///     Converts the string to its camel cased plural form.
        /// </summary>
        /// <param name="text">
        ///     The input string
        /// </param>
        /// <returns>The singular input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWord" | string.camelplural }}
        ///     ```
        ///     ```html
        ///     testWords
        ///     ```
        /// </remarks>
        public static string Camelplural(string text)
        {
            return text?.ToCamelCase().Pluralize();
        }

        /// <summary>
        ///     Converts the string to its snake cased plural form.
        /// </summary>
        /// <param name="text">
        ///     The input string
        /// </param>
        /// <returns>The singular input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWord" | string.snakeplural }}
        ///     ```
        ///     ```html
        ///     test_words
        ///     ```
        /// </remarks>
        public static string Snakeplural(string text)
        {
            return text?.Pluralize().ToSnakeCase();
        }

        /// <summary>
        ///     Converts the string to its pascal cased singular form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The plural input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWords" | string.pascalsingular }}
        ///     ```
        ///     ```html
        ///     TestWord
        ///     ```
        /// </remarks>
        public static string Pascalsingular(string text)
        {
            return text?.ToPascalCase().Singularize();
        }

        /// <summary>
        ///     Converts the string to its camel cased singular form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The plural input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWords" | string.camelsingular }}
        ///     ```
        ///     ```html
        ///     testWord
        ///     ```
        /// </remarks>
        public static string Camelsingular(string text)
        {
            return text?.ToCamelCase().Singularize();
        }

        /// <summary>
        ///     Converts the string to its snake cased singular form.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The plural input string</returns>
        /// <remarks>
        ///     ```scriban-html
        ///     {{ "TestWords" | string.snakesingular }}
        ///     ```
        ///     ```html
        ///     test_word
        ///     ```
        /// </remarks>
        public static string Snakesingular(string text)
        {
            return text?.Singularize().ToSnakeCase();
        }
    }
}