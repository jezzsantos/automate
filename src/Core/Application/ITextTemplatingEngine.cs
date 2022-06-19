using System.Collections;
using Automate.Domain;

namespace Automate.Application
{
    public interface ITextTemplatingEngine
    {
        string Transform(string description, string textTemplate, DraftItem draftItem);

        string Transform(string description, string textTemplate, IDictionary values);
    }
}