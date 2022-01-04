using System;
using System.Collections.Generic;
using Validations.Scopes;

namespace Validations
{
    public class ValidationRunner<T>
    {
        private List<IValidator<T>> Validators { get; } = new();

        public ValidationRunner(List<IValidator<T>> validators)
        {
            Validators = validators;
        }

        public ValidationResult Validate(T obj)
        {
            var context = new ValidationContext<T>();

            foreach (var validator in Validators)
            {
                var scope = validator.GetValidations();

                foreach (var childScope in scope.GetScopes())
                {
                    childScope.Activate(context, obj);
                }

                GetErrors(obj, context, scope);
            }

            return new ValidationResult(context);
        }

        private static void GetErrors(T obj, ValidationContext<T> context, ValidationScope<T> scope)
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
            var context = new ValidationContext<T>();
            var scopeMessages = new List<ValidationMessage>();

            foreach (var validator in Validators)
            {
                var scope = validator.GetValidations();

                foreach (var childScope in scope.GetScopes())
                {
                    childScope.Describe(context); //.Activate(context,).Action.Invoke(context, obj);
                }

                scopeMessages.Add(GetDescriptions(context, scope));
            }

            return new ValidationMessage(scopeMessages);
        }

        private static ValidationMessage GetDescriptions(ValidationContext<T> context, ValidationScope<T> scope)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (scope is null) throw new ArgumentNullException(nameof(scope));
            
            var messages = new List<ValidationMessage>();

            var fieldValidators = scope.GetFieldValidators();
            foreach (var fieldValidator in fieldValidators)
            {
                var fieldMessageGroup = new ValidationMessage(fieldValidator.Property);
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

            return new ValidationMessage(messages);
        }
    }
}