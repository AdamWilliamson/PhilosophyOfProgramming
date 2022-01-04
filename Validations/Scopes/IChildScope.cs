using System;

namespace Validations.Scopes
{
    public interface IChildScope<T>
    {
        void Activate(IValidationContext context, T instance);
        void Describe(IValidationContext context);
    }

    public class PassThroughChildScope<TValidationType, TPassThrough> : IChildScope<TValidationType>
    {
        public PassThroughChildScope(
             Func<IValidationContext, TValidationType, TPassThrough> scoped,
            Action<ScopeInternal<TValidationType, TPassThrough>> rules
        )
        {
            Action = scoped;
            Rules = rules;
        }

        public Action<ScopeInternal<TValidationType, TPassThrough>> Rules { get; }
        public Func<IValidationContext, TValidationType, TPassThrough> Action { get; }

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