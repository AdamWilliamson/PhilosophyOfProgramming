using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Validations.Internal;
using Validations.Validations;

namespace Validations.Scopes
{
    public class ForEachScope<TValidationType, TListItem> : IChildScope<TValidationType>
    {
        readonly List<IChildScope<TValidationType>> childScopes = new();
        private readonly IFieldDescriptor<TValidationType> parentChain;
        private readonly IScope<TValidationType> parentScope;
        protected bool canActivate = true;
        protected Func<IValidationContext, TValidationType, bool>?  canActivateFunc = null;
        public string? Description { get; protected set; } = null;

        public ForEachScope(
            IFieldDescriptor<TValidationType> parentChain,
            IScope<TValidationType> parentScope,
            Action<IFieldDescriptor<TValidationType, TListItem>> rules
        )
        {
            this.parentChain = parentChain;
            this.parentScope = parentScope;
            Rules = rules;
        }

        public IScope<TValidationType> GetParentScope()
        {
            return parentScope;
        }

        public ScopeMessage GetValidationDetails()
        {
            return new ScopeMessage(nameof(ValidationScope<TValidationType>), Description, GetParentScope()?.GetValidationDetails(), new());
        }

        public void AddChildScope(IChildScope<TValidationType> scope) { childScopes.Add(scope); }
        public List<IChildScope<TValidationType>> GetChildScopes() { return childScopes; }

        protected Action<IFieldDescriptor<TValidationType, TListItem>> Rules { get; }

        public void Activate(IValidationContext context, TValidationType instance)
        {
            if (instance == null) { throw new ArgumentNullException(nameof(instance)); }

            var list = parentChain.GetPropertyValue(instance) as IEnumerable<TListItem>;
            if (list == null)
            {
                return;
            }
            var validationScope = parentChain.GetFieldDescriptorScope();

            var methodInfo = typeof(TValidationType).GetMethod(parentChain.Property);
            var propertyInfo = typeof(TValidationType).GetProperty(parentChain.Property);
            if (propertyInfo == null || propertyInfo.GetMethod == null) return;

            var param = Expression.Parameter(typeof(TValidationType));
            var property = Expression.Property(param, propertyInfo.GetMethod);

            for(int x = 0; x < list.Count(); x++)
            {
                var indexed = Expression.Property(property, "Item", Expression.Constant(x));
                Expression<Func<TValidationType, TListItem>> propExpr = Expression.Lambda<Func<TValidationType, TListItem>>(indexed, param);
                Rules.Invoke(validationScope.CreateFieldChainValidator(propExpr));
            }
        }

        public void ActivateToDescribe(IValidationContext context)
        {
            //Rules.Invoke(new ScopeInternal<TValidationType, TPassThrough>());
        }
    }
}