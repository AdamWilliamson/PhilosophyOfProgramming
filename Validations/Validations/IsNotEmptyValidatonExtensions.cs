using System;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;

namespace Validations;

public static partial class FieldChainValidatorExtensions
{
    public static IFieldDescriptor<TClassType, TResult> IsNotEmpty<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain)
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsNotEmptyValidation<TResult>(false)));
        return chain;
    }
}