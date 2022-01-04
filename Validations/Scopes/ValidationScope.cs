using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Validations.Scopes
{
    // Scope of Rules.
    // Useful for both Knowing what threads to use.
    // Also wha information needs to be shared.
    public class ValidationScope<T>
    {
        private List<IChildScope<T>> ChildScope = new();
        private List<IFieldChainValidator<T>> fieldChainValidators = new();

        public ValidationScope() { }

        public void AddScope(IChildScope<T> scope)
        {
            ChildScope.Add(scope);
        }

        public FieldChainValidator<T, TResult> CreateFieldChainValidator<TResult>(Expression<Func<T, TResult>> property)
        {
            var existing = fieldChainValidators.Find(x => x.Matches(property));

            if (existing is FieldChainValidator<T, TResult> converted)
            {
                return converted;
            }

            var validator = new FieldChainValidator<T, TResult>(property);
            fieldChainValidators.Add(validator);

            return validator;
        }

        public List<IFieldChainValidator<T>> GetFieldValidators()
        {
            return fieldChainValidators;
        }

        public List<IChildScope<T>> GetScopes()
        {
            return ChildScope;
        }

        public void Include(ValidationScope<T> scope)
        {
            foreach (var fieldvalidation in scope.GetFieldValidators())
            {
                var copied = false;
                foreach (var currentFieldValidation in GetFieldValidators())
                {
                    if (currentFieldValidation.Property == fieldvalidation.Property)
                    {
                        copied = true;

                        foreach (var validation in fieldvalidation.GetValidations())
                        {
                            currentFieldValidation.AddValidator(validation);
                        }
                        break;
                    }
                }

                if (!copied)
                {
                    fieldChainValidators.Add(fieldvalidation);
                }
            }
        }
    }
}