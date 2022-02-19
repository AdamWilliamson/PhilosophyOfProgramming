using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Validations;

namespace Validations
{
    public class ValidationMessage
    {
        public ScopeMessage? ScopeMessage { get; set; } = null;
        public string? Validator { get; private set; }
        public string? Error { get; set; } = null;

        public List<ValidationMessage> Children { get; set; } = new();

        public ValidationMessage(string? message, Dictionary<string, string> keyToValues) 
        {
            if (message != null)
            {
                var replacer = new StringReplacer(keyToValues);
                Error = replacer.Replace(message);
            }
        }

        public ValidationMessage(string? validator, string? message, Dictionary<string, string> keyToValues)
            :this(message, keyToValues)
        {
            Validator = validator;
        }

        public ValidationMessage(List<ValidationMessage> scopeMessages, ScopeMessage? scopeMessage)
        {
            Children = scopeMessages;
            ScopeMessage = scopeMessage;
        }

        public ValidationMessage(string? validator, ScopeMessage? scopeMessage)
        {
            Validator = validator;
            ScopeMessage = scopeMessage;
        }

        public ValidationMessage(string? validator, string? message, ScopeMessage? scopeMessage, Dictionary<string, string> keyToValues)
            : this(message, keyToValues)
        {
            Validator = validator;
            Error = message;
            ScopeMessage = scopeMessage;
        }
    }
}