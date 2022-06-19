using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automate.Extensions;

namespace Automate.Domain
{
    internal interface IValidateable
    {
        ValidationResults Validate(ValidationContext context, object value);
    }

    public class ValidationResults : Collection<ValidationResult>
    {
        public static ValidationResults None => new();

        public List<ValidationResult> Results => Items.ToList();

        public void AddRange(IEnumerable<ValidationResult> results)
        {
            if (results.Exists())
            {
                results
                    .ToList()
                    .ForEach(result => Items.Add(result));
            }
        }
    }

    public class ValidationResult
    {
        public ValidationResult(ValidationContext context, string message)
        {
            Message = message;
            Context = context;
        }

        public ValidationContext Context { get; }

        public string Message { get; }
    }

    public class ValidationContext
    {
        public ValidationContext(string path)
        {
            path.GuardAgainstNullOrEmpty(nameof(path));
            Path = path;
        }

        public string Path { get; }
    }
}