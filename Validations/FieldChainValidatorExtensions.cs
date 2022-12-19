using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;

namespace Validations
{
    public static partial class FieldChainValidatorExtensions
    {
        public static IFieldDescriptor<TClassType, TResult> When<TClassType, TResult>(
            this IFieldDescriptor<TClassType, TResult> chain, 
            string description,
            Func<IValidationContext, TClassType, bool> validateWhenTrue)
            where TResult : IComparable
        {
            var lastValidation = chain.GetValidations().LastOrDefault();

            if (lastValidation != null)
            {
                _ = chain.GetValidations().Remove(lastValidation);
                //var validationPieces = lastValidation?.GetAllValidations().ToArray() ?? Array.Empty<IValidation>();

                chain.AddValidator(new WhenValidationWrapper<TClassType>(chain.GetCurrentScope(), validateWhenTrue, lastValidation));

                if (description != null)
                {
                    lastValidation?.SetMessageTemplate(description);
                }
            }
            
            return chain;
        }

        public static IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> ForEach<TClassType, TPropertyType>(
            this IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> chain//,
            //Action<ChildAbstractValidator<TClassType, TPropertyType>>? validationsForEachItem
            )
        {
            //if (validationsForEachItem == null) throw new ArgumentNullException(nameof(validationsForEachItem));
            ////if (chain.GetCurrentScope() is not IValidationScope<TClassType> validationScope) throw new ArgumentNullException(nameof(chain), "Field Descriptor has invalid scope type");

            ////var scope = new ForEachScope<TClassType, TPropertyType>(chain, chain.GetCurrentScope(), validationsForEachItem);
            ////validationScope.AddChildScope(scope);
            ////chain.AddValidator(new ValidationWrapper<TPropertyType>(chain.GetCurrentScope(), new ForEachValidation<TPropertyType>(validationsForEachItem)));

            ////chain.AddValidator(new ForEachValidationWrapper(
            ////    chain.GetCurrentScope(),
            ////    validationsForEachItem
            ////    ));

            return chain;
        }


        //public static IFieldDescriptor<TClassType, TResult> ForEach<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain)
        //    where TResult : IComparable
        //{
        //    chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new EqualityValidation(value, true)));
        //    return chain;
        //}

        //public static IFieldDescriptor<TClassType, TResult> IsVitallyEqualTo<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, TResult value)
        //    where TResult : IComparable
        //{
        //    chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new EqualityValidation(value, true)));
        //    return chain;
        //}

        //public static IFieldDescriptor<TClassType, TResult> IsEqualTo<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, TResult value)
        //    where TResult : IComparable
        //{
        //    chain.AddValidator(new ValidationWrapper<TClassType>(chain.GetCurrentScope(), new EqualityValidation(value, false)));
        //    return chain;
        //}

        //public static IFieldDescriptor<TDataType, TResult> IsEqualTo<TDataType, TResult>(
        //    this IFieldDescriptor<TDataType, TResult> chain,
        //    IScopedData scopedData
        //)
        //    where TResult : IComparable
        //{
        //    chain.AddValidator(new ValidationWrapper<TDataType>(chain.GetCurrentScope(), new EqualityValidation(scopedData, false)));
        //    return chain;
        //}

        public static IFieldDescriptor<TClassType, TResult> WithErrorMessage<TClassType, TResult>(this IFieldDescriptor<TClassType, TResult> chain, string message)
        {
            chain.GetValidations().Last().SetMessageTemplate(message);
            return chain;
        }

        public static IFieldDescriptor<T, TResult> Custom<T, TResult>(this IFieldDescriptor<T, TResult> chain, Action<CustomContext, TResult?> custom)
        {
            if (custom == null) throw new ArgumentNullException(nameof(custom));

            chain.AddValidator(new ValidationWrapper<T>(chain.GetCurrentScope(), new CustomValidation<TResult>(custom)));
            return chain;
        }

        //public static void ForEach<TValidationType, TListType, TListItemType>(
        //    this IFieldDescriptor<TValidationType, TListType> chain,
        //    Action<IFieldDescriptor<TValidationType, TListItemType>> rules)
        //    where TValidationType: class
        //    where TListType : IEnumerable<TListItemType>
        //{
        //    //var child = new ForEachScope<TValidationType, TListItemType>(chain, chain.GetCurrentScope(), rules);
        //    //(chain.GetCurrentScope() as ValidationScope<TValidationType>)?.AddChildScope(child);
        //}
    }
}