using System;
using Validations.Internal;
using Validations.Validations;

namespace Validations;

public static partial class FieldChainValidatorExtensions
{
    public static IFieldDescriptor<TClassType, TResult> Is<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, 
        Func<TResult, bool> value,
        string description,
        string error)
        where TResult : IComparable
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsFunctionTestValidation<TResult>(value, description, error)));
        return chain;
    }

    //public static IFieldDescriptor<TDataType, TResult> IsEqualTo<TDataType, TResult>(
    //    this IFieldDescriptor<TDataType, TResult> chain,
    //    IScopedData scopedData
    //)
    //    where TResult : IComparable
    //{
    //    chain.AddValidator(new ValidationWrapper<TDataType>(chain.GetCurrentScope(), new IsEqualToValidation(scopedData, false)));
    //    return chain;
    //}
}