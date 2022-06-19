using System.Collections;
using Automate.Runtime.Domain;

namespace Automate.Common.Application
{
    public interface ITextTemplatingEngine
    {
        string Transform(string description, string textTemplate, DraftItem draftItem);

        string Transform(string description, string textTemplate, IDictionary values);
    }
}