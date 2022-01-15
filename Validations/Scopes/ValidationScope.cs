using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Internal;

namespace Validations.Scopes
{
    public class ValidationScope<TValidationType> 
    {
        private List<IChildScope<TValidationType>> ChildScope = new();
        private List<IFieldChainValidator<TValidationType>> fieldChainValidators = new();

        IChildScope<TValidationType>? currentScope = null;

        public void SetExecutingScope(IChildScope<TValidationType> scope)
        {
            currentScope = scope;
        }

        public void AddScope(IChildScope<TValidationType> scope)
        {
            if (currentScope == null) ChildScope.Add(scope);
            else currentScope.AddScope(scope);
        }

        public IFieldChainValidator<TValidationType> CreateFieldChainValidator<TResult>(Expression<Func<TValidationType, TResult>> property)
        {
            var existing = fieldChainValidators.Find(x => x.Matches(property));

            if (existing is FieldChainValidator<TValidationType, TResult> converted)
            {
                return converted;
            }

            var validator = new FieldChainValidator<TValidationType, TResult>(property);
            fieldChainValidators.Add(validator);

            return validator;
        }

        public List<IFieldChainValidator<TValidationType>> GetFieldValidators()
        {
            return fieldChainValidators;
        }

        public List<IChildScope<TValidationType>> GetScopes()
        {
            return ChildScope;
        }

        public void Include(ValidationScope<TValidationType> scope)
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

            foreach(var childScope in scope.GetScopes())
            {
                this.AddScope(childScope);
            }
        }
    }
}