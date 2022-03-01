using System;
using System.Collections;
using System.Collections.Generic;
using Validations.Scopes;

namespace Validations.Validations;

public class HasLengthBetweenValidation : ValidationBase<Range>
{
    private const string StartToken = "start";
    private const string EndToken = "end";
    public override string Name { get; } = "Is Between";
    public override string DescriptionTemplate { get; } = $"Must be between {StartToken} and {EndToken}";
    public override string MessageTemplate { get; } = $"Is not between {StartToken} and {EndToken}";
    public Range? Value { get; }

    public HasLengthBetweenValidation(Range value, bool isfatal) : base(isfatal)
    {
        Value = value;
    }

    public HasLengthBetweenValidation(IScopedData value, bool isfatal) : base(value, isfatal)
    { }

    protected override Dictionary<string, string> GetTokenValues()
    {
        return new()
        {
            { StartToken, Value?.Start.ToString() ?? "Unknown" },
            { EndToken, Value?.End.ToString() ?? "Unknown" }
        };
    }

    public override bool Test(Range scopedValue, object? instanceValue)
    {
        var value = Equals(scopedValue, default) ? Value : scopedValue;
        if (value == null || instanceValue == default)
        {
            return false;
        }

        switch (instanceValue)
        {
            case null:
            case string s when s.Length > value!.Value.Start.Value  && s.Length < value!.Value.Start.Value:
            case ICollection c when c.Count > value!.Value.Start.Value && c.Count < value!.Value.Start.Value:
            case Array a when a.Length > value!.Value.Start.Value && a.Length < value!.Value.Start.Value:
                return true;
        }

        return false;
    }
}