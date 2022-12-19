//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;

//namespace Validations_Tests.TheNewVersion;

//public static class LanguageManager
//{
//    public static string Resolve(string message, Dictionary<string, string> tokens)
//    {
//        foreach(var pair in tokens)
//        {
//            message = message.Replace(pair.Key, pair.Value);
//        }

//        return message;
//    }
//}

//public interface IFieldValidator
//{
//    string ErrorMessage { get; }
//    string Description { get; }
//    Error? Validate(object? value);
//    Description Describe();

//    Dictionary<string, string> GetTokens();
//}

//public class Error 
//{
//    public string ErrorMessage { get; }

//    public Error(string errorMessage, Dictionary<string,string> tokens) 
//    {
//        ErrorMessage = LanguageManager.Resolve(errorMessage, tokens);
//    }
//}

//public class Description
//{
//    public string ErrorMessage { get; }
//    public string DescriptionMessage { get; }

//    public Description(string descriptionMessage, string errorMessage, Dictionary<string, string> tokens) 
//    {
//        DescriptionMessage = LanguageManager.Resolve(descriptionMessage, tokens);
//        ErrorMessage = LanguageManager.Resolve(errorMessage, tokens);
//    }
//}

//public class IsEqualTo : IFieldValidator 
//{
//    public string ErrorMessage { get; } = "Is Not Equal To {value}";
//    public string Description { get; } = "Should Not Be Equal To {value}";
//    private readonly object? _testValue;

//    public IsEqualTo(object? testValue)
//    {
//        _testValue = testValue;
//    }

//    public Error? Validate(object? value)
//    {
//        if (_testValue == null)
//        {
//            if (value == null) { return null; }
//            else return new Error(ErrorMessage, GetTokens());

//        }

//        return _testValue.Equals(value)? null : new Error(ErrorMessage, GetTokens());
//    }

//    public Description Describe()
//    {
//        return new Description(Description, ErrorMessage, GetTokens());
//    }

//    public Dictionary<string, string> GetTokens()
//    {
//        return new()
//        {
//            { "{value}", _testValue?.ToString() ?? "" }
//        };
//    }
//}

//public static partial class ValidationExtensions
//{
//    public static IFieldDescriptor<TFieldType> IsEqualTo<TFieldType>(this IFieldDescriptor<TFieldType> fieldDescriptor, TFieldType value) 
//    {
//        fieldDescriptor.AddValidation(new IsEqualTo(value));
//        return fieldDescriptor;
//    }
//}

//public static partial class ValidationExtensions
//{
//    public static IFieldDescriptor<TFieldType> Vitally<TFieldType>(this IFieldDescriptor<TFieldType> fieldDescriptor, TFieldType value)
//    {
//        return fieldDescriptor;
//    }

//    public static IFieldDescriptor<TFieldType> WithErrorMessage<TFieldType>(this IFieldDescriptor<TFieldType> fieldDescriptor, string message)
//    {
//        fieldDescriptor.Validations.Last().ErrorMessage = message;
//        return fieldDescriptor;
//    }
//}

//public interface IFieldValidatorWrapper 
//{
//    string ErrorMessage { get; set; }

//    Error? Validate(object? value);
//    Description Describe();
//}

//public class FieldValidatorWrapper : IFieldValidatorWrapper
//{
//    private readonly IFieldValidator fieldValidator;
//    private string? errorMessage;
//    public string ErrorMessage { get { return errorMessage ?? fieldValidator.ErrorMessage; } set { errorMessage = value; } }

//    public FieldValidatorWrapper(IFieldValidator fieldValidator)
//    {
//        this.fieldValidator = fieldValidator;
//    }

//    public Error? Validate(object? value)
//    {
//        return fieldValidator.Validate(value);
//    }

//    public Description Describe()
//    {
//        return fieldValidator.Describe();
//    }
//}

//public interface IValidationComponent 
//{
//    FieldError? Validate(object? value);
//    FieldDescription Describe();
//}

//public interface IFieldDescriptor : IValidationComponent { }

//public interface IFieldDescriptor<TFieldType> : IFieldDescriptor
//{
//    IFieldDescriptor<TFieldType> Vitally { get; }
//    IReadOnlyCollection<IFieldValidatorWrapper> Validations { get; }

//    void AddValidation(IFieldValidator fieldValidator);
//    void AddValidation(IFieldValidatorWrapper fieldValidator);
//}

//public class FieldDescriptor<TFieldType> : IFieldDescriptor<TFieldType>
//{
//    protected bool _IsVitally = false;
//    protected List<IFieldValidatorWrapper> validations = new();
//    public IReadOnlyCollection<IFieldValidatorWrapper> Validations { get { return validations; } }
//    public IFieldDescriptor<TFieldType> Vitally { get { return this; } }
//    public string FieldString { get; }

//    public FieldDescriptor(string fieldString)
//    {
//        FieldString = fieldString;
//    }

//    public void AddValidation(IFieldValidator fieldValidator)
//    {
//        validations.Add(new FieldValidatorWrapper(fieldValidator));
//    }
//    public void AddValidation(IFieldValidatorWrapper fieldValidator)
//    {
//        validations.Add(fieldValidator);
//    }

//    public FieldError? Validate(object? value)
//    {
//        var fieldErrors = new List<Error>();
//        foreach (var validatorWrapper in Validations)
//        {
//            var error = validatorWrapper.Validate(value);

//            if (error != null)
//                fieldErrors.Add(error);
//        }
        
//        if (!fieldErrors.Any()) return null;

//        return new FieldError(FieldString, fieldErrors);
//    }

//    public FieldDescription Describe()
//    {
//        var descriptions = new List<Description>();
//        foreach (var validatorWrapper in Validations)
//        {
//            var description = validatorWrapper.Describe();

//            if (description != null)
//                descriptions.Add(description);
//        }

//        return new FieldDescription(FieldString, descriptions);
//    }
//}

//public interface IWhenContext 
//{
//    bool CanValidate(object validatableObject);
//}
//public class WhenContext<TValidationType> : IWhenContext
//{
//    private readonly Func<TValidationType, bool> _canActivateFunc;
//    //private bool? _canValidate = null;
//    public string Description { get; }

//    public WhenContext(
//        string description,
//        Func<TValidationType, bool> canActivateFunc)
//    {
//        Description = description;
//        _canActivateFunc = canActivateFunc;
//    }

//    private bool CanValidateImpl(TValidationType validatableObject)
//    {
//        return _canActivateFunc.Invoke(validatableObject);
//    }

//    public bool CanValidate(object validatableObject)
//    {
//        if (validatableObject is TValidationType validationType)
//            CanValidateImpl(validationType);

//        return false;
//    }

//}

//public class WhenValidationComponentWrapper<TValidationType, TFieldType> : ValidationComponentWrapper<TValidationType, TFieldType>, IValidationComponentWrapper<TValidationType>
//{
//    private readonly IWhenContext context;

//    public WhenValidationComponentWrapper(
//        IWhenContext context,
//        Expression<Func<TValidationType, TFieldType>> fieldExpression, 
//        IValidationComponent validationComponent) 
//        : base(fieldExpression, validationComponent)
//    {
//        this.context = context;
//    }

//    public override List<IFieldDescriptorWrapper<TValidationType>> Expand(object validatableObject)
//    {
//        if (!context.CanValidate(validatableObject))
//            return new List<IFieldDescriptorWrapper<TValidationType>>() { };

//        return base.Expand(validatableObject);
//    }
//}


//public interface IFieldDescriptorWrapper<TValidationType> 
//{
//    IValidationComponent ValidationComponent { get; }
//    FieldError? Validate(TValidationType valiatableObject);
//    FieldDescription Describe();
//}

//public class FieldDescriptorWrapper<TValidationType, TFieldType> : IFieldDescriptorWrapper<TValidationType>
//{
//    public IValidationComponent ValidationComponent { get; }
//    public Expression<Func<TValidationType, TFieldType>> FieldExpression { get; }

//    public FieldDescriptorWrapper(Expression<Func<TValidationType, TFieldType>> fieldExpression, IValidationComponent validationComponent)
//    {
//        FieldExpression = fieldExpression;
//        ValidationComponent = validationComponent;
//    }

//    public FieldError? Validate(TValidationType valiatableObject)
//    {
//        var fieldValue = FieldExpression.Compile().Invoke(valiatableObject);

//        return ValidationComponent.Validate(fieldValue);
//    }

//    public FieldDescription Describe()
//    {
//        return ValidationComponent.Describe();
//    }
//}

//public interface IValidationComponentWrapper<TValidationType>
//{
//    //IValidationComponent ValidationComponent { get; }
    
//    List<IFieldDescriptorWrapper<TValidationType>> Expand(object validatableObject);
//}

//public class ValidationComponentWrapper<TValidationType, TFieldType> : IValidationComponentWrapper<TValidationType>
//{
//    public IValidationComponent ValidationComponent { get;  }
//    public Expression<Func<TValidationType, TFieldType>> FieldExpression { get; }

//    public ValidationComponentWrapper(Expression<Func<TValidationType, TFieldType>> fieldExpression, IValidationComponent validationComponent)
//    {
//        FieldExpression = fieldExpression;
//        ValidationComponent = validationComponent;
//    }

//    public virtual List<IFieldDescriptorWrapper<TValidationType>> Expand(object validatableObject)
//    {
//        return new List<IFieldDescriptorWrapper<TValidationType>>() { new FieldDescriptorWrapper<TValidationType, TFieldType>(FieldExpression, ValidationComponent) };
//    }
//}

////====  Scopes
//public interface IScope { }
////public class WhenScope<TValidationType>: IValidationComponentWrapper<TValidationType>
////{
////    private readonly Func<TValidationType, bool> _canActivateFunc;
////    protected Action Rules { get; }
////    public string? Description { get; protected set; } = null;

////    public WhenScope(
////        Func<TValidationType, bool> canActivateFunc,
////        Action rules,
////        string? description = null
////    )
////    {
////        _canActivateFunc = canActivateFunc;
////        Rules = rules;
////        Description = description;
////    }

////    protected void ActivateImpl(TValidationType instance)
////    {
////        if (_canActivateFunc?.Invoke(instance) == true)
////        {
////            Rules.Invoke();
////        }
////    }

////    public void Activate(object instance)
////    {
////        if (instance is TValidationType validationType)
////            ActivateImpl(validationType);
////    }

////    //public ValidationMessage ActivateToDescribe()
////    //{
////    //    throw new NotImplementedException();
////    //    Rules.Invoke();
////    //    return new ValidationMessage("", null);
////    //}

////    public List<IFieldDescriptorWrapper<TValidationType>> Expand()
////    {
////        return new List<IFieldDescriptorWrapper<TValidationType>>() {  };
////    }
////}


////====  Validators

//public interface IValidator<TValidationType> 
//{
//    IList<IValidationComponentWrapper<TValidationType>> GetValidationComponents();
//}

//public class AbstractValidator<TValidationType> : IValidator<TValidationType>
//{
//    private readonly List<IValidationComponentWrapper<TValidationType>> _validationComponents = new();

//    public IFieldDescriptor<TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
//    {
//        var name = ExpressionUtilities.GetPropertyPath(expr);
//        var fieldDescriptor = new FieldDescriptor<TFieldType>(name);
//        var item = new ValidationComponentWrapper<TValidationType, TFieldType>(expr, fieldDescriptor);
//        _validationComponents.Add(item);

//        return fieldDescriptor;
//    }

//    public void Scope<TPassThrough>(
//       string description,
//        Func<TValidationType, TPassThrough> scoped,
//        Action rules)
//    {
    
//    }

//    public void Include(ISubValidator<TValidationType> validatorToInclude) { }

//    public void When(
//        string whenDescription,
//        Func<TValidationType, bool> ifTrue,
//        Action rules)
//    {
//        var context = new WhenContext<TValidationType>(whenDescription, ifTrue);
//        var item = new WhenValidationComponentWrapper<TValidationType, TFieldType>(context, rules);
//        _validationComponents.Add(item);
//    }

//    public void ScopeWhen<TPassThrough>(
//        string whenDescription,
//        Func<TValidationType, bool> ifTrue,
//        Func<TValidationType, TPassThrough> scoped,
//        Action rules)
//    { }

//    IList<IValidationComponentWrapper<TValidationType>> IValidator<TValidationType>.GetValidationComponents()
//    {
//        return _validationComponents;
//    }
//}

////====  Sub Validators
//public interface ISubValidator<TValidationType> : IValidator<TValidationType> {}
//public abstract class AbstractSubValidator<TValidationType> : AbstractValidator<TValidationType>, ISubValidator<TValidationType> {}

////==== 
//public class FieldError
//{
//    public string FieldName { get; }
//    public List<Error> ErrorMessages { get; }

//    public FieldError(string fieldName, List<Error> errorMessages)
//    {
//        FieldName = fieldName;
//        ErrorMessages = errorMessages;
//    }
//}

//public class FieldDescription
//{
//    public string FieldName { get; set; }
//    public List<Description> Descriptions { get; set; }

//    public FieldDescription(string fieldName, List<Description> descriptions)
//    {
//        FieldName = fieldName;
//        Descriptions = descriptions;
//    }
//}

//public class ValidationResult
//{
//    public List<FieldError> Errors { get; set; } = new();

//    internal void Merge(ValidationResult validationResult)
//    {
//        foreach(var error in validationResult.Errors)
//        {
//            if (Errors.SingleOrDefault(f => f.FieldName == error.FieldName) is { } existing)
//            {
//                existing.ErrorMessages.AddRange(error.ErrorMessages);
//            }
//            else
//            {
//                Errors.Add(error);
//            }
//        }
//    }

//    internal void Merge(FieldError fieldError)
//    {
//        if (Errors.SingleOrDefault(f => f.FieldName == fieldError.FieldName) is { } existing)
//        {
//            existing.ErrorMessages.AddRange(fieldError.ErrorMessages);
//        }
//        else
//        {
//            Errors.Add(fieldError);
//        }
//    }
//}

//public class DescriptionResult
//{
//    public List<FieldDescription> Descriptions { get; set; } = new();

//    internal void Merge(FieldDescription fieldError)
//    {
//        if (Descriptions.SingleOrDefault(f => f.FieldName == fieldError.FieldName) is { } existing)
//        {
//            existing.Descriptions.AddRange(fieldError.Descriptions);
//        }
//        else
//        {
//            Descriptions.Add(fieldError);
//        }
//    }
//}

//public class ValidationRunner<TValidationType>
//{
//    private readonly IEnumerable<IValidator<TValidationType>> validators;
//    private readonly Dictionary<string, LinkedList<IFieldDescriptor>> fieldDescriptors = new();
    
//    private LinkedList<IFieldDescriptor> GetFieldDescriptorList(string field)
//    {
//        if (!fieldDescriptors.ContainsKey(field))
//        {
//            fieldDescriptors.Add(field, new LinkedList<IFieldDescriptor>());
//        }

//        return fieldDescriptors[field];
//    }

//    public ValidationRunner(IEnumerable<IValidator<TValidationType>> validators)
//    {
//        this.validators = validators;
//    }

//    public ValidationResult Validate(TValidationType validationObject)
//    {
//        var result = new ValidationResult();
//        var validationComponents = new List<IValidationComponentWrapper<TValidationType>>();
//        var fieldDescriptors = new List<IFieldDescriptorWrapper<TValidationType>>();

//        // Get components from validators
//        foreach (var validator in validators)
//        {
//            var components = ((IValidator<TValidationType>)validator).GetValidationComponents();

//            validationComponents.AddRange(components);
//        }

//        // Get FieldDescriptors from Components
//        foreach (var component in validationComponents)
//        {
//            var descriptors = component.Expand();

//            fieldDescriptors.AddRange(descriptors);
//        }

//        // Get errors from field descriptors
//        foreach (var component in fieldDescriptors)
//        {
//            var componentResult = component.Validate(validationObject);

//            if (componentResult is not null)
//                result.Merge(componentResult);
//        }

//        return result;
//    }

//    public DescriptionResult Describe(Album album)
//    {
//        var result = new DescriptionResult();
//        var validationComponents = new List<IValidationComponentWrapper<TValidationType>>();
//        var fieldDescriptors = new List<IFieldDescriptorWrapper<TValidationType>>();

//        // Get components from validators
//        foreach (var validator in validators)
//        {
//            var components = ((IValidator<TValidationType>)validator).GetValidationComponents();

//            validationComponents.AddRange(components);
//        }

//        // Get FieldDescriptors from Components
//        foreach (var component in validationComponents)
//        {
//            var descriptors = component.Expand();

//            fieldDescriptors.AddRange(descriptors);
//        }

//        // Get errors from field descriptors
//        foreach (var component in fieldDescriptors)
//        {
//            var componentResult = component.Describe();

//            if (componentResult is not null)
//                result.Merge(componentResult);
//        }

//        return result;
//    }
//}