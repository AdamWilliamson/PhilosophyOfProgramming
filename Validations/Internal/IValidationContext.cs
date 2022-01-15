using System;
using System.Collections.Generic;
using System.Linq;

namespace Validations.Internal
{
    public interface IValidationContext
    {
        Dictionary<string, List<ValidationError>> GetErrors();
        void AddError(string property, string message, bool fatal = false);
        void AddError(string message, bool fatal = false);
    }

    public class ValidationContext<T> : IValidationContext
    {
        private readonly Dictionary<string, List<ValidationError>> Errors = new();
        protected string? CurrentProperty { get; set; }

        public ValidationContext() { }

        public void SetCurrentProperty(string property) { CurrentProperty = property; }

        public Dictionary<string, List<ValidationError>> GetErrors() { return Errors; }

        public void AddError(string property, string message, bool fatal = false)
        {
            List<ValidationError> errors;
            if (Errors.ContainsKey(property))
            {
                errors = Errors[property];
            }
            else
            {
                errors = new List<ValidationError>();
                Errors.Add(property, errors);
            }

            errors.Add(new ValidationError(message, fatal));
        }

        public void AddError(string message, bool fatal = false)
        {
            if (string.IsNullOrWhiteSpace(CurrentProperty))
            {
                throw new InvalidOperationException("Current Property Empty");
            }

            var property = CurrentProperty ?? "";
            AddError(property, message, fatal);
        }

        internal bool HasFatalError(string property)
        {
            if (Errors.ContainsKey(property))
            {
                if (Errors[property] != null)
                {
                    if (Errors[property].Any(e => e.IsFatal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}