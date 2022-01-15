using System.Collections.Generic;
using Validations;
using System.Linq;
using Validations.Internal;

namespace Validation_Tests
{
    public static class ValidationTestExtensions
    {
        public static List<ValidationError> GetErrorsForField(this ValidationResult? results, string fieldName)
        {
            return results?.GetErrors()[fieldName] ?? new List<ValidationError>();
        }

        public static bool ContainsError(this ValidationResult? results, string fieldName, string error)
        {
            return results?.GetErrorsForField(fieldName)?.Any(e => e.Error == error) ?? false;
        }
    }
}