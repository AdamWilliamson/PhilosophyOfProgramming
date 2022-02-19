using System.Collections.Generic;
using Validations.Internal;

namespace Validations
{
    public class FieldErrors
    {
        public string Property { get; set; } = string.Empty;
        public List<ValidationError> Errors { get; set; } = new();
    }

    public class ValidationResult
    {
        public List<FieldErrors> FieldErrors { get; } = new();

        public ValidationResult(List<FieldErrors> errors) { FieldErrors = errors; }
    }
}