using System;

namespace Validations.Validations
{
    public class CustomValidation<T> : IValidation
        where T : class
    {
        public string DescriptionTemplate { get; } = "";
        public string MessageTemplate { get; } = "Custom";
        public Action<IValidationContext, T?> Custom { get; }

        public CustomValidation(Action<IValidationContext, T?> custom)
        {
            Custom = custom;
        }

        public void Validate<TOther>(ValidationContext<TOther> context, object? value)
        {
            Custom.Invoke(context, value as T);
        }

        public ValidationMessage Describe<TOther>(ValidationContext<TOther> context)
        {
            return new ValidationMessage(DescriptionTemplate);
        }
    }
}