using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Scopes;

namespace Validations
{
    public interface IValidationRunner<TValidationType>
    {
        ValidationResult Validate(TValidationType obj);
        ValidationMessage Describe();
    }

    public class ValidationRunner<TValidationType> : IValidationRunner<TValidationType>
    {
        private List<IValidator<TValidationType>> Validators { get; } = new();

        public ValidationRunner(List<IValidator<TValidationType>> validators)
        {
            Validators = validators;
        }

        public ValidationResult Validate(TValidationType instance)
        {
            var context = new ValidationContext<TValidationType>();

            foreach (var validator in Validators)
            {
                var scope = validator.GetValidations();
                
                foreach (var childScope in scope.GetScopes())
                {
                    RecurseScopes(context, scope, childScope, instance);
                }

                GetErrors(instance, context, scope);
            }

            return new ValidationResult(context);
        }

        private void RecurseScopes(
            ValidationContext<TValidationType> context, 
            ValidationScope<TValidationType> owningScope, 
            IChildScope<TValidationType> currentScope,
            TValidationType instance) 
        {
            owningScope.SetExecutingScope(currentScope);
            currentScope.Activate(context, instance);
            foreach (var childScope in currentScope.GetChildScopes())
            {
                RecurseScopes(context, owningScope, childScope, instance);
            }
        }

        private void GetErrors(TValidationType obj, ValidationContext<TValidationType> context, ValidationScope<TValidationType> scope)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (scope is null) throw new ArgumentNullException(nameof(scope));

            var fieldValidators = scope.GetFieldValidators();
            foreach (var fieldValidator in fieldValidators)
            {
                context.SetCurrentProperty(fieldValidator.Property);
                var fieldValue = fieldValidator.GetPropertyValue(obj);

                var validations = fieldValidator.GetValidations();
                foreach (var validation in validations)
                {
                    foreach (var validationpieces in validation.GetValidations(context, obj))
                    {
                        validationpieces.Validate(context, fieldValue);
                    }
                    if (context.HasFatalError(fieldValidator.Property)) { break; }
                }
            }
        }

        public ValidationMessage Describe()
        {
            var context = new ValidationContext<TValidationType>();
            var scopeMessages = new List<ValidationMessage>();

            foreach (var validator in Validators)
            {
                var scope = validator.GetValidations();

                foreach (var childScope in scope.GetScopes())
                {
                    childScope.Describe(context); //.Activate(context,).Action.Invoke(context, obj);
                }

                scopeMessages.AddRange(GetDescriptions(context, scope));
            }

            return new ValidationMessage(nameof(ValidationRunner<TValidationType>), scopeMessages);
        }

        private List<ValidationMessage> GetDescriptions(ValidationContext<TValidationType> context, ValidationScope<TValidationType> scope)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (scope is null) throw new ArgumentNullException(nameof(scope));
            
            var messages = new List<ValidationMessage>();

            var fieldValidators = scope.GetFieldValidators();
            foreach (var fieldValidator in fieldValidators)
            {
                var fieldMessageGroup = new ValidationMessage(null, fieldValidator.Property);
                messages.Add(fieldMessageGroup);

                context.SetCurrentProperty(fieldValidator.Property);

                var validations = fieldValidator.GetValidations();
                foreach (var validation in validations)
                {
                    foreach (var validationpieces in validation.GetAllValidations())
                    {
                        fieldMessageGroup.Children.Add(validationpieces.Describe(context));
                    }
                }
            }

            return messages;
        }
    }
}