using System;
using System.Collections.Generic;
using Validations.Internal;

namespace Validations.Scopes
{
    public interface IChildScope<TValidationType>
    {
        void AddScope(IChildScope<TValidationType> scope);
        List<IChildScope<TValidationType>> GetChildScopes();
        void Activate(IValidationContext context, TValidationType instance);
        void Describe(IValidationContext context);
    }

    public class PassThroughChildScope<TValidationType, TPassThrough> : IChildScope<TValidationType>
    {
        List<IChildScope<TValidationType>> childScopes = new();

        public PassThroughChildScope(
             Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules
        )
        {
            Action = scoped;
            Rules = rules;
        }

        public void AddScope(IChildScope<TValidationType> scope) { childScopes.Add(scope); }
        public List<IChildScope<TValidationType>> GetChildScopes() { return childScopes; }

        protected Action<ScopeInternal<TValidationType, TPassThrough>> Rules { get; }
        protected Func<IValidationContext, TValidationType, TPassThrough> Action { get; }

        public void Activate(IValidationContext context, TValidationType instance)
        {
            var result = Action.Invoke(context, instance);
            Rules.Invoke(new ScopeInternal<TValidationType, TPassThrough>(result));
        }

        public void Describe(IValidationContext context)
        {
            Rules.Invoke(new ScopeInternal<TValidationType, TPassThrough>());
        }
    }
}