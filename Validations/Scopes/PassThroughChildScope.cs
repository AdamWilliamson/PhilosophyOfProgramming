using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Validations;

namespace Validations.Scopes
{
    public class PassThroughChildScope<TValidationType, TPassThrough> : IChildScope<TValidationType>
    {
        readonly List<IChildScope<TValidationType>> childScopes = new();
        private readonly IParentScope parentScope;
        protected bool canActivate = true;
        protected Func<IValidationContext, TValidationType, bool>?  canActivateFunc = null;
        public string? Description { get; protected set; } = null;

        public PassThroughChildScope(
            IParentScope parentScope,
             Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules, 
            string? description = null
        )
        {
            this.parentScope = parentScope;
            Action = scoped;
            Rules = rules;
            Description = description;
        }

        public IParentScope GetParentScope()
        {
            return parentScope;
        }

        public ScopeMessage GetValidationDetails()
        {
            return new ScopeMessage(nameof(ValidationScope<TValidationType>), Description, GetParentScope()?.GetValidationDetails(), new());
        }

        public PassThroughChildScope(
            IParentScope parentScope,
            Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules,
            bool canActivate,
            Func<IValidationContext, TValidationType, bool>? canActivateFunc,
            string? description = null
        ) : this(parentScope, scoped, rules, description)
        {
            this.canActivate = canActivate;
            this.canActivateFunc = canActivateFunc;
        }

        public void AddChildScope(IChildScope<TValidationType> scope) { childScopes.Add(scope); }
        public List<IChildScope<TValidationType>> GetChildScopes() { return childScopes; }

        protected Action<ScopeInternal<TValidationType, TPassThrough>> Rules { get; }
        protected Func<IValidationContext, TValidationType, TPassThrough> Action { get; }

        public void Expand(IValidationContext context, object instance)
        {
            throw new NotImplementedException();
        }

        public void ExpandToDescribe(IValidationContext context)
        {
            throw new NotImplementedException();
        }

        public void Activate(IValidationContext context, object instance)
        {
            if (instance is TValidationType validationInstance)
                Activate(context, validationInstance);
        }

        protected void Activate(IValidationContext context, TValidationType instance)
        {
            if (canActivate || canActivateFunc?.Invoke(context, instance) == true)
            {
                var result = Action.Invoke(context, instance);
                Rules.Invoke(new ScopeInternal<TValidationType, TPassThrough>(result));
            }
        }

        public ValidationMessage ActivateToDescribe(IValidationContext context)
        {
            Rules.Invoke(new ScopeInternal<TValidationType, TPassThrough>());

            return new ValidationMessage("", null);
        }
    }
}