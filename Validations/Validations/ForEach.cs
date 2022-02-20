//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Validations.Internal;

//namespace Validations.Validations;

//public class ForEachValidation<T> : IValidation
//{
//    public string Name { get; } = "For Each";
//    public string DescriptionTemplate { get; } = "";
//    public string MessageTemplate { get; } = "";
//    public Action<CustomContext, T?> Custom { get; }

//    public ForEachValidation(Action<CustomContext, T?> custom)
//    {
//        Custom = custom;
//    }

//    public ValidationError? Validate<TOther>(ValidationContext<TOther> context, object? value)
//    {
//        if (value is T converted)
//        {
//            var customContext = new CustomContext();
//            Custom.Invoke(customContext, converted);

//            return new ValidationError("Custom", false, new())
//            {
//                ChildErrors = customContext.GetErrors().Select(e => new ValidationError(e.Item1, e.Item2, new())).ToList()
//            };
//        }

//        return null;
//    }

//    public ValidationMessage Describe<TOther>(ValidationContext<TOther> context)
//    {
//        return new ValidationMessage("Custom", DescriptionTemplate, new());
//    }
//}