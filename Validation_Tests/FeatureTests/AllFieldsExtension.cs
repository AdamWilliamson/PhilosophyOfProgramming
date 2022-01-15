using System;
using Validations;

namespace Validations_Tests
{
    public static class AllFieldsExtension
    {
        public static IFieldDescriptor<T, TResult> AddAllIComparableValidators<T, TResult>(this IFieldDescriptor<T, TResult> describe)
            where TResult: IComparable
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return describe.IsEqualTo(default(TResult));
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }

        public static IFieldDescriptor<T, TResult> AddAllValidatorsRequiringAClassOwner<T, TResult>(this IFieldDescriptor<T, TResult> describe)
            where T: class
        {
            return describe.Custom((context, instance) => {
                context.AddError("Bad thing happened");
            });
        }
    }
}
