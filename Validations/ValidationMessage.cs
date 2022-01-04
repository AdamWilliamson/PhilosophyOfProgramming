using System.Collections.Generic;

namespace Validations
{
    //=======================================
    // Validations
    public class ValidationMessage
    {
        public string Message { get; set; } = string.Empty;
        public List<ValidationMessage> Children { get; set; } = new();

        public ValidationMessage(string message)
        {
            Message = message;
        }

        public ValidationMessage(List<ValidationMessage> scopeMessages)
        {
            Children = scopeMessages;
        }

        public ValidationMessage(string message, List<ValidationMessage> scopeMessages)
        {
            Message = message;
            Children = scopeMessages;
        }
    }
}