using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Validations.Validations;

namespace Validations
{
    public interface IFieldChainValidator<T>
    {
        void AddValidator(IValidationWrapper<T> validation);
        List<IValidationWrapper<T>> GetValidations();
        object? GetPropertyValue(object instance);
        string Property { get; }
        bool Matches<TResult>(Expression<Func<T, TResult>> action);
    }

    public class FieldChainValidator<T, TResult> : IFieldChainValidator<T>
    {
        private List<IValidationWrapper<T>> Validations { get; } = new();
        public Expression<Func<T, TResult>> PropertyExpr { get; }
        public PropertyInfo? propInfo { get; }
        public string Property { get; }

        public FieldChainValidator(Expression<Func<T, TResult>> property)
        {
            if (property == null || property.Body == null)
                throw new ArgumentNullException(nameof(property));

            PropertyExpr = property;

            MemberExpression? member = property.Body as MemberExpression;
            propInfo = member?.Member as PropertyInfo;
            if (propInfo == null)
                throw new InvalidOperationException();

            Property = propInfo.Name;
        }

        public List<IValidationWrapper<T>> GetValidations() { return Validations; }

        public bool Matches<TResult2>(Expression<Func<T, TResult2>> action)
        {
            MemberExpression? member = action.Body as MemberExpression;
            var propInfo = member?.Member as PropertyInfo;
            if (propInfo == null)
                throw new InvalidOperationException();

            return Property == propInfo.Name;
        }

        public object? GetPropertyValue(object instance)
        {
            return propInfo?.GetValue(instance);
        }

        public void AddValidator(IValidationWrapper<T> validation)
        {
            Validations.Add(validation);
        }
    }
}