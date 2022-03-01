using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Internal;
using Validations.Scopes;

namespace Validations
{
    public interface IValidator<T>
    {
        ValidationScope<T> GetScope();
    }

    /// <summary>
    /// Inheritable Facade around the ValidationScope
    /// Supplies handy functions from the Validation Scope, but hides the validation scope.
    /// Provides for a cleaner more adaptable interface for dealing with generating Field validations.
    /// </summary>
    /// <typeparam name="TValidationType"></typeparam>
    public class AbstractValidator<TValidationType> : IValidator<TValidationType>
    {
        ValidationScope<TValidationType> validationScope = new ValidationScope<TValidationType>();

        protected IFieldDescriptor<TValidationType, TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
        {
            return validationScope.CreateFieldChainValidator(expr);
        }

        protected void Scope<TPassThrough>(
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules)
        {
            var child = new PassThroughChildScope<TValidationType, TPassThrough>(validationScope, scoped, rules);
            validationScope.AddChildScope(child);
        }

        protected void ForEachScope<TListItemType>(
            Expression<Func<TValidationType, IEnumerable<TListItemType>>> expr,
            Action<IFieldDescriptor<TValidationType, TListItemType>> listItemRules)
            //where TListType : IEnumerable<TListItemType>
        {
            var chain = Describe(expr);
            var child = new ForEachScope<TValidationType, TListItemType>(chain, chain.GetCurrentScope(), listItemRules);
            validationScope.AddChildScope(child);
        }

        protected void ForEachScope</*TListType,*/ TListItemType>(
            Expression<Func<TValidationType, IEnumerable<TListItemType>>> expr,
            Action<IFieldDescriptor<TValidationType, IEnumerable<TListItemType>>> propertyRules,
            Action<IFieldDescriptor<TValidationType, TListItemType>> listItemRules)
            //where TListType : IEnumerable<TListItemType>
        {
            var chain = Describe(expr);
            propertyRules.Invoke(chain);
            var child = new ForEachScope<TValidationType, TListItemType>(chain, chain.GetCurrentScope(), listItemRules);
            validationScope.AddChildScope(child);
        }

        protected void Include(IValidator<TValidationType> validatorToInclude)
        {
            validationScope.Include(validatorToInclude.GetScope());
        }

        protected void When(
            string whenDescription,
            Func<IValidationContext, TValidationType, bool> ifTrue,
            Action rules)
        {
            var child = new WhenScope<TValidationType>(validationScope, rules, false, ifTrue, whenDescription);
            validationScope.AddChildScope(child);
        }

        protected void ScopeWhen<TPassThrough>(
            string whenDescription,
            Func<IValidationContext, TValidationType, bool> ifTrue,
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules)
        {
            var child = new PassThroughChildScope<TValidationType, TPassThrough>(validationScope, scoped, rules, false, ifTrue, whenDescription);
            validationScope.AddChildScope(child);
        }

        ValidationScope<TValidationType> IValidator<TValidationType>.GetScope()
        {
            return validationScope;
        }
    }
}