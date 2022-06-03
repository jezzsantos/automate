using System.Collections;

namespace Automate.CLI.Domain
{
    internal interface ITextTemplatingEngine
    {
        string Transform(string description, string textTemplate, DraftItem draftItem);

        string Transform(string description, string textTemplate, IDictionary values);
    }
}