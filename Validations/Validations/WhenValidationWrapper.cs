using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Scopes;

namespace Validations.Validations
{
    public class WhenValidationWrapper<TValidationType> : ValidationWrapper<TValidationType>
    {
        private readonly Func<IValidationContext, TValidationType, bool> validateWhenTrue;

        public WhenValidationWrapper(IScope<TValidationType> owningScope, Func<IValidationContext, TValidationType, bool> validateWhenTrue, params IValidation[] validations)
            : base(owningScope, validations)
        {
            this.validateWhenTrue = validateWhenTrue;
        }

        public override List<IValidation> GetValidations(ValidationContext<TValidationType> validationContext, TValidationType instance)
        {
            if (validateWhenTrue.Invoke(validationContext, instance))
                return validations;

            return new List<IValidation>();
        }
    }
}