using System;
using System.Collections.Generic;
using System.Linq;
using Validations.Internal;

namespace Validations.Validations;

public class CustomContext
{
    private readonly List<(string, bool)> errors = new();
        
    public CustomContext() { }

    public List<(string, bool)> GetErrors() { return errors; }

    public void AddError(string message, bool fatal = false)
    {
        errors.Add(new (message, fatal));
    }
}

public class CustomValidation<T> : IValidation
{
    public string Name { get; } = "Custom";
    public string DescriptionTemplate { get; } = "";
    public string MessageTemplate { get; } = "Custom";
    public Action<CustomContext, T?> Custom { get; }

    protected bool isFatal = false;
    public CustomValidation(Action<CustomContext, T?> custom)
    {
        Custom = custom;
    }

    public ValidationError? Validate<TOther>(ValidationContext<TOther> context, object? value)
    {
        if (value is T converted)
        {
            var customContext = new CustomContext();
            Custom.Invoke(customContext, converted);

            return new ValidationError("Custom", isFatal, new())
            {
                ChildErrors = customContext.GetErrors().Select(e => new ValidationError(e.Item1, e.Item2, new())).ToList()
            };
        }

        return null;
    }

    public ValidationMessage Describe<TOther>(ValidationContext<TOther> context)
    {
        return new ValidationMessage("Custom", DescriptionTemplate, MessageTemplate, new());
    }


    public void IsFatal(bool isFatal)
    {
        this.isFatal = isFatal;
    }
}