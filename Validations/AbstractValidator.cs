using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Validations.Internal;
using Validations.Scopes;
using Validations.Utilities;

namespace Validations
{
    //public interface IValidator { }

    public interface IScopedValidator<T>
    {
        IValidationScope<T> GetScope();

        LinkedList<IValidationObject> Expand(IValidationContext context, object instance);
        LinkedList<IValidationObject> ExpandToDescribe(IValidationContext context);
    }

    public interface IValidator<TValidationType> : IScopedValidator<TValidationType> 
    {
        IFieldDescriptor<TValidationType, TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr);
        void Scope<TPassThrough>(Func<IValidationContext, TValidationType, TPassThrough> scoped, Action<ScopeInternal<TValidationType, TPassThrough>> rules);
        //void ForEachScope<TListItemType>(Expression<Func<TValidationType, IEnumerable<TListItemType>>> expr, Action<ChildAbstractValidator<TValidationType, TListItemType>> listItemRules);
        void Include(IScopedValidator<TValidationType> validatorToInclude);

        void When(
            string whenDescription,
            Func<IValidationContext, TValidationType, bool> ifTrue,
            Action rules);

        void ScopeWhen<TPassThrough>(
            string whenDescription,
            Func<IValidationContext, TValidationType, bool> ifTrue,
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules);
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

        public IFieldDescriptor<TValidationType, TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
        {
            return validationScope.GetFieldDescriptor(expr);
        }

        protected IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> DescribeEnumerable<TFieldType>(Expression<Func<TValidationType, IEnumerable<TFieldType>>> expr)
        {
            return validationScope.GetFieldDescriptor(expr);
        }

        public LinkedList<IValidationObject> Expand(IValidationContext context, object instance) 
        {
            validationScope.Expand(context, instance);
            return validationScope.GetValidationObjects();
        }

        public LinkedList<IValidationObject> ExpandToDescribe(IValidationContext context) 
        {
            validationScope.ExpandToDescribe(context);
            return validationScope.GetValidationObjects();
        }

        public void Scope<TPassThrough>(
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules)
        {
            var child = new PassThroughChildScope<TValidationType, TPassThrough>(validationScope, scoped, rules);
            validationScope.AddValidatableObject(child);
        }

        //protected IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> DescribeEnumerable<TFieldType>(Expression<Func<TValidationType, IEnumerable<TFieldType>>> expr)

        //public static IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> ForEach<TClassType, TPropertyType>(
        //   this IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> chain,
        //   Action<ChildAbstractValidator<TClassType, TPropertyType>>? validationsForEachItem
        //   )

        //public void ForEachScope<TListItemType>(
        //    Expression<Func<TValidationType, IEnumerable<TListItemType>>> expr,
        //    Action<ChildAbstractValidator<TValidationType, TListItemType>> listItemRules)
        //{
        //    var chain = Describe(expr);
        //    var child = new ForEachScope<TValidationType, TListItemType>(chain, chain.GetCurrentScope(), listItemRules);
        //    validationScope.AddChildScope(child);
        //}

        //protected void ForEachScope</*TListType,*/ TListItemType>(
        //    Expression<Func<TValidationType, IEnumerable<TListItemType>>> expr,
        //    Action<IFieldDescriptor<TValidationType, IEnumerable<TListItemType>>> propertyRules,
        //    Action<AbstractValidator<TListItemType>> listItemRules)
        ////where TListType : IEnumerable<TListItemType>
        //{
        //    var chain = Describe(expr);
        //    propertyRules.Invoke(chain);
        //    var child = new ForEachScope<TValidationType, TListItemType>(chain, chain.GetCurrentScope(), listItemRules);
        //    validationScope.AddChildScope(child);
        //}

        public void Include(IScopedValidator<TValidationType> validatorToInclude)
        {
            //validationScope.Include(validatorToInclude.GetScope());
        }

        public void When(
            string whenDescription,
            Func<IValidationContext, TValidationType, bool> ifTrue,
            Action rules)
        {
            var child = new WhenScope<TValidationType>(validationScope, rules, false, ifTrue, whenDescription);
            validationScope.AddValidatableObject(child);
        }

        public void ScopeWhen<TPassThrough>(
            string whenDescription,
            Func<IValidationContext, TValidationType, bool> ifTrue,
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules)
        {
            var child = new PassThroughChildScope<TValidationType, TPassThrough>(validationScope, scoped, rules, false, ifTrue, whenDescription);
            validationScope.AddValidatableObject(child);
        }

        IValidationScope<TValidationType> IScopedValidator<TValidationType>.GetScope()
        {
            return validationScope;
        }
    }

    //public class ChildAbstractValidator<TOriginValidatorType, TValidationType> : IValidator<TValidationType>
    //{
    //    private readonly IValidationScope<TOriginValidatorType> parent;

    //    //private readonly IScope parent;

    //    public ChildAbstractValidator(
    //        IValidationScope<TOriginValidatorType> parent,
    //        int index
    //        )
    //    {
    //        this.parent = parent;
    //        //this.parent = parent;
    //    }

    //    public new IFieldDescriptor<TValidationType, TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> property)
    //    {
    //        string propertyString = ExpressionUtilities.GetPropertyPath(property);

    //        var existing = parent.GetFieldDescriptor<TFieldType>(propertyString);

    //        if (existing is IFieldDescriptor<TValidationType, TFieldType> converted)
    //        {
    //            return converted;
    //        }

    //        var validator = new ChildFieldDescriptor<TOriginValidatorType, TValidationType, TFieldType>(parent, propertyString);
    //        parent.AddFieldDescriptor(validator);

    //        return (IFieldDescriptor<TValidationType, TFieldType>)validator;
    //    }

    //    //public void ForEachScope<TListItemType>(
    //    //    Expression<Func<TValidationType, IEnumerable<TListItemType>>> property,
    //    //    Action<ChildAbstractValidator<TValidationType, TListItemType>> listItemRules)
    //    //{
            
    //    //}

    //    public new void Scope<TPassThrough>(
    //        Func<IValidationContext, TValidationType, TPassThrough> scoped,
    //        Action<ScopeInternal<TValidationType, TPassThrough>> rules)
    //    {
    //        parent.Scope(scoped, rules);
    //    }

    //    public new void Include(IScopedValidator<TValidationType> validatorToInclude)
    //    {
    //        parent.Include(validatorToInclude);
    //    }

    //    public new void When(
    //        string whenDescription,
    //        Func<IValidationContext, TValidationType, bool> ifTrue,
    //        Action rules)
    //    {
    //        parent.When(whenDescription, ifTrue, rules);
    //    }

    //    public new void ScopeWhen<TPassThrough>(
    //        string whenDescription,
    //        Func<IValidationContext, TValidationType, bool> ifTrue,
    //        Func<IValidationContext, TValidationType, TPassThrough> scoped,
    //        Action<ScopeInternal<TValidationType, TPassThrough>> rules)
    //    {
    //        parent.ScopeWhen(whenDescription, ifTrue, scoped, rules);
    //    }

    //    ValidationScope<TValidationType> IScopedValidator<TValidationType>.GetScope()
    //    {
    //        return validationScope;
    //    }
    //}
}