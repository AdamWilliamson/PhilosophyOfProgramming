using System;
using System.Collections.Generic;
using System.Linq;
using Validations.Scopes;

namespace Validations.Internal
{
    public interface IValidationContext
    {
        //Dictionary<string, List<ValidationError>> GetErrors();
        //void AddError(string property, string message, bool fatal = false);
        //void AddError(string message, bool fatal = false);
    }

    public class ValidationContext<TValidationType> : IValidationContext
    {
        //private readonly Dictionary<string, List<ValidationError>> Errors = new();
        protected string? CurrentProperty { get; set; }
        //private List<string> scopeMessages { get; } = new();
        //public void PushScopeMessage(string msg) { scopeMessages.Add(msg); }
        //public void PopScopeMessage() { scopeMessages.Remove(scopeMessages.Last()); }

        //private List<IScope<TValidationType>> scopes { get; } = new();
        //public void PushScope(IScope<TValidationType> scope) { scopes.Add(scope); }
        //public void PopScope() { scopes.Remove(scopes.Last()); }
        //public IScope<TValidationType> CurrentScope() { return scopes.Last(); }

        public ValidationContext() { }

        public void SetCurrentProperty(string property) { CurrentProperty = property; }

        //public Dictionary<string, List<ValidationError>> GetErrors() { return Errors; }

        //private void AddError(string property, string message, bool fatal = false)
        //{
        //    List<ValidationError> errors;
        //    if (Errors.ContainsKey(property))
        //    {
        //        errors = Errors[property];
        //    }
        //    else
        //    {
        //        errors = new List<ValidationError>();
        //        Errors.Add(property, errors);
        //    }

        //    errors.Add(new ValidationError(message, fatal) { ScopeMessages = scopeMessages.ToList() });
        //}

        //public void AddError(string message, bool fatal = false)
        //{
        //    if (string.IsNullOrWhiteSpace(CurrentProperty))
        //    {
        //        throw new InvalidOperationException("Current Property Empty");
        //    }

        //    var property = CurrentProperty ?? "";
        //    AddError(property, message, fatal);
        //}

        //internal bool HasFatalError(string property)
        //{
        //    if (Errors.ContainsKey(property))
        //    {
        //        if (Errors[property] != null)
        //        {
        //            if (Errors[property].Any(e => e.IsFatal))
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
    }
}