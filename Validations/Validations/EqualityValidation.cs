using System;
using Validations.Scopes;

namespace Validations.Validations
{
    public class EqualityValidation : ValidationBase
    {
        public override string DescriptionTemplate { get; } = "Must equal to {value}";
        public override string MessageTemplate { get; } = "{value} is not equal to {value}";
        public IComparable? Value { get; }

        public EqualityValidation(IComparable value, bool isfatal) : base(isfatal)
        {
            Value = value;
        }

        public EqualityValidation(IScopedData value, bool isfatal) : base(value, isfatal)
        { }

        public override bool Test(object? internalValue, object? instanceValue)
        {
            return internalValue == null && instanceValue == null || internalValue?.Equals(instanceValue) == true;
        }
    }
}