using System;
using Validations.Internal;

namespace Validations.Validations
{
    public class CustomValidation<T> : IValidation
     //   where T : class
    {
        public string Name { get; } = "Custom";
        public string DescriptionTemplate { get; } = "";
        public string MessageTemplate { get; } = "Custom";
        public Action<IValidationContext, T?> Custom { get; }

        public CustomValidation(Action<IValidationContext, T?> custom)
        {
            Custom = custom;
        }

        public void Validate<TOther>(ValidationContext<TOther> context, object? value)
        {
            if (value is T converted)
                Custom.Invoke(context, converted);
        }

        public ValidationMessage Describe<TOther>(ValidationContext<TOther> context)
        {
            return new ValidationMessage("Custom", DescriptionTemplate);
        }
    }
}