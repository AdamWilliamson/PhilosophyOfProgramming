using System;
using System.Linq.Expressions;
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

        protected FieldChainValidator<TValidationType, TResult> Describe<TResult>(Expression<Func<TValidationType, TResult>> expr)
        {
            return parentScope.CreateFieldChainValidator(expr);
        }

        protected void Scope<TPassThrough>(
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules)
        {
            var child = new PassThroughChildScope<TValidationType, TPassThrough>(scoped, rules);
            parentScope.AddScope(child);
        }

        public void Include(IValidator<TValidationType> validatorToInclude)
        {
            parentScope.Include(validatorToInclude.GetValidations());
        }

        public ValidationScope<TValidationType> GetValidations()
        {
            return parentScope;
        }
    }
}