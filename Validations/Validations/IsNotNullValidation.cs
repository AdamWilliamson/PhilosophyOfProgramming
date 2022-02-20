using System;
using System.Collections.Generic;

namespace Validations.Validations;

public class IsNotNullValidation : ValidationBase<IComparable>
{
    public override string Name { get; } = "Is Not Null";
    public override string DescriptionTemplate { get; } = $"Must not be Null";
    public override string MessageTemplate { get; } = $"Is Null";

    public IsNotNullValidation(bool isfatal) : base(isfatal) {}

    protected override Dictionary<string, string> GetTokenValues() { return new(); }

    public override bool Test(IComparable? scopedValue, object? instanceValue)
    {
        return scopedValue != null || instanceValue != null;
    }
}
