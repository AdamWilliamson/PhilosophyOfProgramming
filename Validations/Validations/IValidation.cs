using System;
using Validations.Scopes;

namespace Validations.Validations
{
    public interface IValidation
    {
        string MessageTemplate { get; }
        string DescriptionTemplate { get; }
        void Validate<T>(ValidationContext<T> context, object? value);
        ValidationMessage Describe<T>(ValidationContext<T> context);
    }

    public abstract class ValidationBase : IValidation
    {
        public abstract string MessageTemplate { get; }
        public abstract string DescriptionTemplate { get; }
        protected bool isfatal = false;
        public IScopedData? ScopedData = null;
        protected ValidationBase(bool isfatal)
        {
            this.isfatal = isfatal;
        }
        protected ValidationBase(IScopedData value, bool isfatal)
        {
            ScopedData = value;
            this.isfatal = isfatal;
        }

        protected TValue? GetValue<T, TValue>(ValidationContext<T> context, TValue value)
        {
            if (ScopedData == null) return value;

            if (ScopedData.GetValue() is TValue result)
                return result;

            return default;
        }

        public virtual void Validate<T>(ValidationContext<T> context, object? value)
        {
            if (Test(GetValue(context, value), value))
            {
                return;
            }

            context.AddError(MessageTemplate, isfatal);
        }

        public virtual ValidationMessage Describe<T>(ValidationContext<T> context)
        {
            var data = "";
            if (ScopedData != null)
            {
                data = ScopedData.Describe();
            }

            return new ValidationMessage(DescriptionTemplate + data);
        }

        public abstract bool Test(object? internalValue, object? instanceValue);
    }
}