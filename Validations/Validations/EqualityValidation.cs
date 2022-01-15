using System;
using Validations.Scopes;

namespace Validations.Validations
{
    public class EqualityValidation : ValidationBase<IComparable>
    {
        public override string Name { get; } = "Equal To";
        public override string DescriptionTemplate { get; } = "Must equal to {value}";
        public override string MessageTemplate { get; } = "{value} is not equal to {value}";
        public IComparable? Value { get; }

        public EqualityValidation(IComparable value, bool isfatal) : base(isfatal)
        {
            Value = value;
        }

        public EqualityValidation(IScopedData value, bool isfatal) : base(value, isfatal)
        { }

        public override bool Test(IComparable? scopedValue, object? instanceValue)
        {
            if (scopedValue == null && Value != null)
            {
                return Value.Equals(instanceValue);
            }

            return scopedValue == null && instanceValue == null || scopedValue?.Equals(instanceValue) == true;
        }
    }
}