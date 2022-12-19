using System;
using System.Collections.Generic;
using Validations.Internal;
using Validations.Scopes;

namespace Validations.Validations
{
    public class WhenValidationWrapper<TValidationType> : ValidationWrapper<TValidationType>
    {
        private readonly Func<IValidationContext, TValidationType, bool> validateWhenTrue;
        private readonly IValidationWrapper<TValidationType> wrapper;

        public WhenValidationWrapper(
            IScope<TValidationType> owningScope, 
            Func<IValidationContext, TValidationType, bool> validateWhenTrue, 
            IValidationWrapper<TValidationType> wrapper)
            : base(owningScope, Array.Empty<IValidation>())
        {
            this.validateWhenTrue = validateWhenTrue;
            this.wrapper = wrapper;
        }

        public override List<IValidation> GetValidations(ValidationContext<TValidationType> validationContext, TValidationType instance)
        {
            if (validateWhenTrue.Invoke(validationContext, instance))
                return wrapper.GetValidations(validationContext, instance);

            return new List<IValidation>();
        }
    }
}