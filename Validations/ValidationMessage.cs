using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Validations;

namespace Validations
{
    public class MessageFormatter
    {
        private class Message
        {
            public string Description { get; set; }

            public Message(string description, string errorMessage)
            {
                Description = description;
                ErrorMessage = errorMessage;
            }

            public string ErrorMessage { get; set; }
        }

        private static readonly Lazy<MessageFormatter> lazy = new(() => new MessageFormatter());

        public static MessageFormatter Instance { get { return lazy.Value; } }

        private MessageFormatter() {}

        private Dictionary<string, Dictionary<string, Message>> replacementMessages = new() { { "en", new Dictionary<string, Message>() } };
        public string Language { get; set; } = "en";

        public void ReplaceMessage(string language, string owner, string errorMessage, string description)
        {
            if (!replacementMessages.ContainsKey(language))
            {
                replacementMessages.Add(language, new Dictionary<string, Message>());
            }

            if (replacementMessages[language].ContainsKey(owner))
            {
                replacementMessages[language].Remove(owner);
            }

            replacementMessages[language].Add(owner, new Message(description: description, errorMessage: errorMessage));
        }

        public string FormatError(string owner, string message, Dictionary<string, string> keyToValues)
        {
            var realMessage = 
                replacementMessages.ContainsKey(Language) && replacementMessages[Language].ContainsKey(owner)
                ? replacementMessages[Language][owner].ErrorMessage 
                : message;

            var replacer = new StringReplacer(keyToValues);
            return replacer.Replace(realMessage);
        }

        public string FormatDescription(string owner, string message, Dictionary<string, string> keyToValues)
        {
            var realMessage =
                replacementMessages.ContainsKey(Language) && replacementMessages[Language].ContainsKey(owner)
                ? replacementMessages[Language][owner].Description
                : message;

            var replacer = new StringReplacer(keyToValues);
            return replacer.Replace(realMessage);
        }
    }

    public class ValidationMessage
    {
        public ScopeMessage? ScopeMessage { get; set; } = null;
        public string? Validator { get; private set; }
        public string? Error { get; set; } = null;
        public string? Description { get; set; } = null;

        public List<ValidationMessage> Children { get; set; } = new();

        public ValidationMessage(string owner, string? description, string? error, Dictionary<string, string> keyToValues) 
        {
            if (description != null)
            {
                Description = MessageFormatter.Instance.FormatDescription(owner, description, keyToValues);
            }

            if (error != null)
            {
                Error = MessageFormatter.Instance.FormatError(owner, error, keyToValues);
            }
        }

        public ValidationMessage(List<ValidationMessage> scopeMessages, ScopeMessage? scopeMessage)
        {
            Children = scopeMessages;
            ScopeMessage = scopeMessage;
        }

        public ValidationMessage(string validator, ScopeMessage? scopeMessage)
        {
            Validator = validator;
            ScopeMessage = scopeMessage;
        }

        public ValidationMessage(string validator, string? description, string? error, ScopeMessage? scopeMessage, Dictionary<string, string> keyToValues)
            : this(validator, description, error, keyToValues)
        {
            Validator = validator;
            ScopeMessage = scopeMessage;
        }
    }
}