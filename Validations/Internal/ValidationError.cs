using System.Collections.Generic;

namespace Validations.Internal
{
    public class ValidationError
    {
        public string Error { get; }
        public bool IsFatal { get; } = false;
        public List<ValidationError> ChildErrors { get; set; } = new();

//        public List<string> ScopeMessages { get; set; } = new();

  //      public ValidationError(IMessageScope currentScope, string error) : this(currentScope, error, false) { }

        public ValidationError(string error, bool fatal, Dictionary<string, string> keyToValues)
        {
            IsFatal = fatal;

            var replacer = new StringReplacer(keyToValues);
            Error = replacer.Replace(error);
        }

    }
}