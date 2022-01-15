using System;
using System.Linq.Expressions;
using Validations.Internal;
using Validations.Scopes;

namespace Validations
{
    public interface IValidator<T>
    {
        ValidationScope<T> GetValidations();
    }

    public class AbstractValidator<TValidationType> : IValidator<TValidationType>
    {
        ValidationScope<TValidationType> parentScope = new();

        protected IFieldDescriptor<TValidationType, TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
        {
            return new FieldDescriptor<TValidationType, TFieldType>(parentScope.CreateFieldChainValidator(expr));
        }

        protected void Scope<TPassThrough>(
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules)
        {
            var child = new PassThroughChildScope<TValidationType, TPassThrough>(scoped, rules);
            parentScope.AddScope(child);
        }

        protected void Include(IValidator<TValidationType> validatorToInclude)
        {
            parentScope.Include(validatorToInclude.GetValidations());
        }

        public ValidationScope<TValidationType> GetValidations()
        {
            return parentScope;
        }
    }
}