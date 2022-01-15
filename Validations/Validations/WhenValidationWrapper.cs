using System;
using System.Collections.Generic;
using Validations.Internal;

namespace Validations.Validations
{
    public class WhenValidationWrapper<T> : ValidationWrapper<T>
    {
        private readonly Func<IValidationContext, T, bool> validateWhenTrue;

        public WhenValidationWrapper(Func<IValidationContext, T, bool> validateWhenTrue, params IValidation[] validations)
            : base(validations)
        {
            this.validateWhenTrue = validateWhenTrue;
        }

        public override List<IValidation> GetValidations(ValidationContext<T> validationContext, T instance)
        {
            if (validateWhenTrue.Invoke(validationContext, instance))
                return validations;

            return new List<IValidation>();
        }
    }
}