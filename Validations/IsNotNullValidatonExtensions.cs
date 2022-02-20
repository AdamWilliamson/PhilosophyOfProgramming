using Validations.Internal;
using Validations.Validations;

namespace Validations;

public static partial class FieldChainValidatorExtensions
{
    public static IFieldDescriptor<TClassType, TResult> IsVitallyNotNull<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain)
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsNotNullValidation(true)));
        return chain;
    }

    public static IFieldDescriptor<TClassType, TResult> IsNotNull<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain)
    {
        chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new IsNotNullValidation(false)));
        return chain;
    }
}