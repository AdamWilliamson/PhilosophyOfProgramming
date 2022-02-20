using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Validations;

namespace Validations.Scopes
{
    public class WhenScope<TValidationType> : IChildScope<TValidationType>
    {
        List<IChildScope<TValidationType>> childScopes = new();
        protected bool canActivate = true;
        protected Func<IValidationContext, TValidationType, bool>? canActivateFunc = null;
        private readonly IScope<TValidationType> parentScope;

        public string? Description { get; protected set; } = null;

        public WhenScope(
            IScope<TValidationType> parentScope,
            Action rules,
            string? description = null
        )
        {
            this.parentScope = parentScope;
            Rules = rules;
            Description = description;
        }

        public WhenScope(
            IScope<TValidationType> parentScope,
            Action rules,
            bool canActivate,
            Func<IValidationContext, TValidationType, bool>? canActivateFunc,
            string? description = null
        ) : this(parentScope, rules, description)
        {
            this.canActivate = canActivate;
            this.canActivateFunc = canActivateFunc;
        }

        public IScope<TValidationType> GetParentScope() { return parentScope; }

        public ScopeMessage GetValidationDetails()
        {
            return new ScopeMessage(nameof(ValidationScope<TValidationType>), Description, GetParentScope()?.GetValidationDetails(), new());
        }

        public void AddChildScope(IChildScope<TValidationType> scope) { childScopes.Add(scope); }
        public List<IChildScope<TValidationType>> GetChildScopes() { return childScopes; }

        protected Action Rules { get; }

        public void Activate(IValidationContext context, TValidationType instance)
        {
            if (canActivate || canActivateFunc?.Invoke(context, instance) == true)
            {
                Rules.Invoke();
            }
        }

        public void ActivateToDescribe(IValidationContext context)
        {
            Rules.Invoke();
        }
    }
}