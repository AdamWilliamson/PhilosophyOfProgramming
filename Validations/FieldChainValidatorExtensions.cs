using System;
using System.Linq;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;

namespace Validations
{
    public static class FieldChainValidatorExtensions
    {
        public static IFieldDescriptor<TClassType, TResult> When<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, Func<IValidationContext, TClassType, bool> validateWhenTrue)
            where TResult : IComparable
        {
            var lastValidation = chain.GetValidations().LastOrDefault();

            if (lastValidation != null)
            {
                _ = chain.GetValidations().Remove(lastValidation);
                var validationPieces = lastValidation?.GetAllValidations().ToArray() ?? Array.Empty<IValidation>();

                chain.AddValidator(new WhenValidationWrapper<TClassType>(validateWhenTrue, validationPieces));
            }

            return chain;
        }

        public static IFieldDescriptor<TClassType, TResult> IsVitallyEqualTo<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, TResult value)
            where TResult : IComparable
        {
            chain.AddValidator(new ValidationWrapper<TClassType>(new EqualityValidation(value, true)));
            return chain;
        }

        public static IFieldDescriptor<TClassType, TResult> IsEqualTo<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, TResult value)
            where TResult : IComparable
        {
            chain.AddValidator(new ValidationWrapper<TClassType>(new EqualityValidation(value, false)));
            return chain;
        }

        public static IFieldDescriptor<TDataType, TResult> IsEqualTo<TDataType, TResult>(
            this IFieldDescriptor<TDataType, TResult> chain,
            IScopedData scopedData
        )
            where TResult : IComparable
        {
            chain.AddValidator(new ValidationWrapper<TDataType>(new EqualityValidation(scopedData, false)));
            return chain;
        }

        public static IFieldDescriptor<TClassType, TResult> WithMessage<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, string message)
        {
            chain.GetValidations().Last().SetMessageTemplate(message);
            return chain;
        }

        public static IFieldDescriptor<T, TResult> Custom<T, TResult>(this IFieldDescriptor<T, TResult> chain, Action<IValidationContext, TResult?> custom)
        {
            if (custom == null) throw new ArgumentNullException(nameof(custom));

            chain.AddValidator(new ValidationWrapper<T>(new CustomValidation<TResult>(custom)));
            return chain;
        }
    }
}