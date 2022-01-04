using System;
using System.Linq;
using Validations.Scopes;
using Validations.Validations;

namespace Validations
{
    public static class FieldChainValidatorExtensions
    {
        public static FieldChainValidator<T, TResult> When<T, TResult>(this FieldChainValidator<T, TResult> chain, Func<IValidationContext, T, bool> validateWhenTrue)
            where TResult : IComparable
        {
            var lastValidation = chain.GetValidations().LastOrDefault();

            if (lastValidation != null)
            {
                _ = chain.GetValidations().Remove(lastValidation);
                var validationPieces = lastValidation?.GetAllValidations().ToArray() ?? Array.Empty<IValidation>();

                chain.AddValidator(new WhenValidationWrapper<T>(validateWhenTrue, validationPieces));
            }

            return chain;
        }

        public static FieldChainValidator<T, TResult> IsVitallyEqualTo<T, TResult>(this FieldChainValidator<T, TResult> chain, TResult value)
            where TResult : IComparable
        {
            chain.AddValidator(new ValidationWrapper<T>(new EqualityValidation(value, true)));
            return chain;
        }

        public static FieldChainValidator<T, TResult> IsEqualTo<T, TResult>(this FieldChainValidator<T, TResult> chain, TResult value)
            where TResult : IComparable
        {
            chain.AddValidator(new ValidationWrapper<T>(new EqualityValidation(value, false)));
            return chain;
        }

        public static FieldChainValidator<TDataType, TResult> IsEqualTo<TDataType, TResult>(
            this FieldChainValidator<TDataType, TResult> chain,
            IScopedData scopedData
        )
            where TResult : IComparable
        {
            chain.AddValidator(new ValidationWrapper<TDataType>(new EqualityValidation(scopedData, false)));
            return chain;
        }

        public static FieldChainValidator<T, TResult> WithMessage<T, TResult>(this FieldChainValidator<T, TResult> chain, string message)
        {
            chain.GetValidations().Last().SetMessageTemplate(message);
            return chain;
        }

        public static FieldChainValidator<T, TResult> Custom<T, TResult>(this FieldChainValidator<T, TResult> chain, Action<IValidationContext, T?> custom)
            where T: class
        {
            if (custom == null) throw new ArgumentNullException(nameof(custom));

            chain.AddValidator(new ValidationWrapper<T>(new CustomValidation<T>(custom)));
            return chain;
        }
    }
}