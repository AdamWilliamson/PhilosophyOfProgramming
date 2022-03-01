using System.Collections.Generic;
using System.Linq;
using Validations.Internal;
using Validations.Scopes;

namespace Validations.Validations
{
    public class ScopeMessage
    {
        public ScopeMessage? ParentsMessage { get; }
        public string? Message { get; }
        public string ScopeType { get; }

        public ScopeMessage(string scopeType, string? message, ScopeMessage? parentsMessage, Dictionary<string, string> keyToValues)
        {
            ScopeType = scopeType;
            
            ParentsMessage = parentsMessage;

            if (message != null)
            {
                var replacer = new StringReplacer(keyToValues);
                Message = replacer.Replace(message);
            }
        }
    }

    public interface IValidationWrapper<TValidationType>
    {
        ValidationMessage MessageTemplate { get; }
        List<IValidation> GetValidations(ValidationContext<TValidationType> validationContext, TValidationType instance);
        List<IValidation> GetAllValidations();
        void SetMessageTemplate(string messageTemplate);

        //TODO: See if I can refactor this out. <TOther>
        ValidationMessage Describe(ValidationContext<TValidationType> context);
        void MakeVital();
        //IValidationScope<TValidationType> GetOwningScope();
    }

    /// <summary>
    /// A Wrapper around IValidations.
    /// It allows 1 or many IValidations to be grouped together as a single Validation.
    /// It also handles some of the more basic management to reduce IValidations needing duplicate code.
    /// </summary>
    /// <typeparam name="TValidationType">The Class that will be validated against</typeparam>
    public class ValidationWrapper<TValidationType> : IValidationWrapper<TValidationType>
    {
        protected List<IValidation> validations = new();
        protected string? message = null;
        protected IScope<TValidationType> owningScope;

        public virtual ValidationMessage MessageTemplate
        {
            get
            {
                if (this.validations == null || validations.Count == 0)
                {
                    return new ValidationMessage("null", message, null, owningScope.GetValidationDetails(), new());
                }
                else if (validations.Count == 1)
                {
                    var validator = validations.First();
                    return new ValidationMessage(validator.Name, validator.DescriptionTemplate, validator.MessageTemplate, owningScope.GetValidationDetails(), new());
                }
                else
                {
                    var children = validations.Select(x => new ValidationMessage(x.Name, x.DescriptionTemplate, x.MessageTemplate, new())).ToList();
                    return new ValidationMessage(children, owningScope.GetValidationDetails());
                }
            }
        }

        public ValidationWrapper(IScope<TValidationType> owningScope, params IValidation[] validations)
        {
            this.validations = validations.ToList();
            this.owningScope = owningScope;
        }

        public virtual List<IValidation> GetValidations(ValidationContext<TValidationType> validationContext, TValidationType instance)
        {
            return validations;
        }

        public virtual List<IValidation> GetAllValidations() { return validations; }

        public virtual void SetMessageTemplate(string messageTemplate) { message = messageTemplate; }
        //public IValidationScope<TValidationType> GetOwningScope() {  return owningScope; }

        public ValidationMessage Describe(ValidationContext<TValidationType> context)
        {
            var validationpieces = GetAllValidations();
            var scopeDescription = owningScope.GetValidationDetails();

            var scopeMessages = owningScope.GetValidationDetails();

            if (validationpieces.Count == 1)
            {
                return validationpieces.First().Describe(context);
            }

            var messages = new List<ValidationMessage>();
            foreach (var validationpiece in validationpieces)
            {
                var childMessage = validationpiece.Describe(context);

                if (childMessage != null)
                    messages.Add(childMessage);
            }

            return new ValidationMessage(messages, owningScope.GetValidationDetails());
        }

        public void MakeVital()
        {
            foreach(var validation in GetAllValidations())
            {
                validation.IsFatal(true);
            }
        }
    }
}