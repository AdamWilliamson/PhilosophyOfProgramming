using System;
using System.Collections.Generic;
using Validations.Internal;

namespace Validations.Validations;

public class IsNullValidation : ValidationBase<IComparable>
{
    public override string Name { get; } = "Is Null";
    public override string DescriptionTemplate { get; } = $"Must be Null";
    public override string MessageTemplate { get; } = $"Is not Null";

    public IsNullValidation(bool isfatal) : base(isfatal) {}

    protected override Dictionary<string, string> GetTokenValues() { return new(); }

    public override bool Test(IComparable? scopedValue, object? instanceValue)
    {
        return scopedValue == null && instanceValue == null;
    }
}

public static partial class FieldChainValidatorExtensions
{
    public static IFieldDescriptor<TClassType, TResult> IsVitallyNull<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain)
        where TResult : IComparable
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsNullValidation(true)));
        return chain;
    }

    public static IFieldDescriptor<TClassType, TResult> IsNull<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain)
        where TResult : IComparable
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsNullValidation(false)));
        return chain;
    }
}