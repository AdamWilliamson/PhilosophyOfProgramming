using System;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;

namespace Validations;

public static partial class FieldChainValidatorExtensions
{
    public static IFieldDescriptor<TClassType, TResult> IsEqualTo<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, TResult value)
        where TResult : IComparable
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsEqualToValidation(value, false)));
        return chain;
    }

    public static IFieldDescriptor<TDataType, TResult> IsEqualTo<TDataType, TResult>(
        this IFieldDescriptor<TDataType, TResult> chain,
        IScopedData scopedData
    )
        where TResult : IComparable
    {
        chain.AddValidator(new ValidationWrapper<TDataType>(chain.GetCurrentScope(), new IsEqualToValidation(scopedData, false)));
        return chain;
    }
}