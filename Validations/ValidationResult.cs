using System.Collections.Generic;
using Validations.Internal;

namespace Validations
{
    public class ValidationResult
    {
        private Dictionary<string, List<ValidationError>> Errors { get; } = new();

        public ValidationResult(IValidationContext context) { Errors = context.GetErrors(); }

        public Dictionary<string, List<ValidationError>> GetErrors()
        {
            return Errors;
        }
    }
}