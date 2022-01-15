using System.Collections.Generic;

namespace Validations
{
    public class ValidationMessage
    {
        public string? Validator { get; private set; }
        public string? Message { get; set; } = null;
        public List<ValidationMessage> Children { get; set; } = new();

        public ValidationMessage(string? validator, string? message)
        {
            Validator = validator;
            Message = message;
        }

        public ValidationMessage(List<ValidationMessage> scopeMessages)
        {
            Children = scopeMessages;
        }

        public ValidationMessage(string? validator, List<ValidationMessage> scopeMessages)
        {
            Validator = validator;
            Children = scopeMessages;
        }

        public ValidationMessage(string? validator, string? message, List<ValidationMessage> scopeMessages)
        {
            Validator = validator;
            Message = message;
            Children = scopeMessages;
        }
    }
}