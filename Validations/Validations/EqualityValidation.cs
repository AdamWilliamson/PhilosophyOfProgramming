using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Scopes;

namespace Validations.Validations;

public class EqualityValidation : ValidationBase<IComparable>
{
    private const string ValueToken = "value";
    public override string Name { get; } = "Equal To";
    public override string DescriptionTemplate { get; } = $"Must equal to {ValueToken}";
    public override string MessageTemplate { get; } = $"Is not equal to {ValueToken}";
    public IComparable? Value { get; }

    public EqualityValidation(IComparable value, bool isfatal) : base(isfatal)
    {
        Value = value;
    }

    public EqualityValidation(IScopedData value, bool isfatal) : base(value, isfatal)
    { }

    protected override Dictionary<string, string> GetTokenValues()
    {
        return new()
        {
            { ValueToken, Value?.ToString() ?? ScopedData?.GetValue()?.ToString() ?? "Unknown" }
        };
    }

    public override bool Test(IComparable? scopedValue, object? instanceValue)
    {
        if (scopedValue == null && Value != null)
        {
            return Value.Equals(instanceValue);
        }

        return scopedValue == null && instanceValue == null || scopedValue?.Equals(instanceValue) == true;
    }
}