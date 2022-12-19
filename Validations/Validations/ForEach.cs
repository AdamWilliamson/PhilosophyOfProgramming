//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Validations.Internal;

//namespace Validations.Validations;

//public class ForEachValidation<TPropertyType> : IValidation
//{
//    private readonly IValidator parent;

//    public string Name { get; } = "For Each";
//    public string DescriptionTemplate { get; } = "";
//    public string ErrorTemplate { get; } = "";
    
//    public string MessageTemplate { get; } = "";
//    public Action<ChildAbstractValidator<TPropertyType>> Custom { get; }

//    public ForEachValidation(IValidator parent, Action<ChildAbstractValidator<TPropertyType>> custom)
//    {
//        this.parent = parent;
//        Custom = custom;
//    }

//    public ValidationError? Validate<TOther>(ValidationContext<TOther> context, object? value)
//    {
//        if (value is IEnumerable<TPropertyType> converted)
//        {
//            var validator = new ChildAbstractValidator<TPropertyType>(parent);

//            foreach (var item in converted)
//            {
//                Custom.Invoke(validator);

//                return new ValidationError("Custom", false, new())
//                {
//                    ChildErrors = customContext.GetErrors().Select(e => new ValidationError(e.Item1, e.Item2, new())).ToList()
//                };
//            }
//        }

//        return null;
//    }

//    public ValidationMessage Describe<TOther>(ValidationContext<TOther> context)
//    {
//        return new ValidationMessage("Custom", DescriptionTemplate, ErrorTemplate, new());
//    }
//}