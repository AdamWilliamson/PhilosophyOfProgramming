using System.Collections.Generic;
using Validations.Internal;
using Validations.Scopes;

namespace Validations.Validations
{
    public interface IValidation
    {
        string Name { get; }
        string MessageTemplate { get; }
        string DescriptionTemplate { get; }
        ValidationError? Validate<T>(ValidationContext<T> context, object? value);
        ValidationMessage Describe<T>(ValidationContext<T> context);
        void IsFatal(bool isFatal);
    }

    public abstract class ValidationBase<TFieldType> : IValidation
    {
        public abstract string Name { get; }
        public abstract string MessageTemplate { get; }
        public abstract string DescriptionTemplate { get; }
        protected abstract Dictionary<string, string> GetTokenValues();

        protected bool isFatal = false;
        public IScopedData? ScopedData = null;

        protected ValidationBase(bool isFatal)
        {
            this.isFatal = isFatal;
        }
        protected ValidationBase(IScopedData value, bool isFatal)
        {
            ScopedData = value;
            this.isFatal = isFatal;
        }

        public void IsFatal(bool isFatal)
        {
            this.isFatal = isFatal;
        }

        protected TValue? GetValue<T, TValue>(ValidationContext<T> context)
        {
            if (ScopedData == null) return default;

            if (ScopedData.GetValue() is TValue result)
                return result;

            return default;
        }

        public virtual ValidationError? Validate<T>(ValidationContext<T> context, object? value)
        {
            if (Test(GetValue<T, TFieldType>(context), value))
            {
                return null;
            }

            return new ValidationError(MessageTemplate, isFatal, GetTokenValues());
        }

        public virtual ValidationMessage Describe<T>(ValidationContext<T> context)
        {
            return new ValidationMessage(Name, DescriptionTemplate, MessageTemplate, GetTokenValues());
        }

        public abstract bool Test(TFieldType? scopedValue, object? instanceValue);
    }
}