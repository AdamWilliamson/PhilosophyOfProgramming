using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Utilities;
using Validations.Validations;

namespace Validations.Internal
{
    public interface IFieldChainValidator<TClassType>
    {
        void AddValidator(IValidationWrapper<TClassType> validation);
        List<IValidationWrapper<TClassType>> GetValidations();
        object? GetPropertyValue(object instance);
        string Property { get; }
        bool Matches<TResult>(Expression<Func<TClassType, TResult>> action);
    }

    internal sealed class FieldChainValidator<TClassType, TResult> : IFieldChainValidator<TClassType>
    {
        private List<IValidationWrapper<TClassType>> Validations { get; } = new();
        private Expression<Func<TClassType, TResult>> propertyAcessor;
        public string Property { get; }

        public FieldChainValidator(Expression<Func<TClassType, TResult>> property)
        {
            if (property == null || property.Body == null)
                throw new ArgumentNullException(nameof(property));

            Property = ExpressionUtilities.GetPropertyPath(property);
            propertyAcessor = property;
        }

        public List<IValidationWrapper<TClassType>> GetValidations() { return Validations; }

        public bool Matches<TResult2>(Expression<Func<TClassType, TResult2>> action)
        {
            return Property == ExpressionUtilities.GetPropertyPath(action);
        }

        public object? GetPropertyValue(object instance)
        {
            if (instance is TClassType convert)
            {
                return propertyAcessor.Compile().Invoke(convert);
            }
            return null;
        }

        public void AddValidator(IValidationWrapper<TClassType> validation)
        {
            Validations.Add(validation);
        }
    }
}