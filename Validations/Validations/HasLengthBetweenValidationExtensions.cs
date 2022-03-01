using System;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;

namespace Validations;

public static partial class FieldChainValidatorExtensions
{
    public static IFieldDescriptor<TClassType, TResult> HasLengthBetween<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, int start, int end)
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new HasLengthBetweenValidation(new Range(start, end), false)));
        return chain;
    }

    public static IFieldDescriptor<TDataType, TResult> HasLengthBetween<TDataType, TResult>(
        this IFieldDescriptor<TDataType, TResult> chain,
        IScopedData<Range> scopedData
    )
        where TResult : IComparable
    {
        chain.AddValidator(new ValidationWrapper<TDataType>(chain.GetCurrentScope(), new HasLengthBetweenValidation(scopedData, false)));
        return chain;
    }
}