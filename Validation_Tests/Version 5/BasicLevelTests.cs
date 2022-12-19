using ApprovalTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Validations_Tests.Version_5;

public static class ExpressionUtilities
{
    public static MemberExpression? GetMemberExpression(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            return memberExpression;
        }
        else if (expression is LambdaExpression lambdaExpression)
        {
            if (lambdaExpression?.Body is MemberExpression expression1)
            {
                return expression1;
            }
            else if (lambdaExpression?.Body is UnaryExpression expression3)
            {
                return (MemberExpression)expression3.Operand;
            }
        }
        return null;
    }

    public static string GetPropertyPath(Expression expr)
    {
        var path = new StringBuilder();
        MemberExpression? memberExpression = GetMemberExpression(expr);
        if (memberExpression != null && memberExpression.Expression != null)
        {

            do
            {
                if (path.Length > 0)
                {
                    path.Insert(0, ".");
                }
                path.Insert(0, memberExpression.Member.Name);
#pragma warning disable CS8604 // Possible null reference argument.
                memberExpression = GetMemberExpression(memberExpression.Expression);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            while (memberExpression != null);
            return path.ToString();
        }
        else if (GetIndexPath(expr) is string name)
        {
            return name;
        }
        return "";
    }

    public static Func<TOwner, TPropertyType> GetAccessor<TOwner, TPropertyType>(string path)
    {
        ParameterExpression paramObj = Expression.Parameter(typeof(TOwner), "obj");

        Expression body = paramObj;
        foreach (string property in path.Split('.'))
        {
            body = Expression.PropertyOrField(body, property);
        }

        return Expression.Lambda<Func<TOwner, TPropertyType>>(body, new ParameterExpression[] { paramObj }).Compile();
    }

    public static string GetIndexPath(Expression expression)
    {
        string path = "";
        if (expression is LambdaExpression lambdaExpression)
        {
            if (lambdaExpression?.Body is IndexExpression indexExpression)
            {
                if (indexExpression.Arguments.Count == 1 && indexExpression.Arguments[0] is ConstantExpression constantExpression)
                {
                    path = $"[{constantExpression.Value}]";
                    if (indexExpression.Object is MemberExpression memberExpression)
                    {
                        var temp = GetPropertyPath(memberExpression);
                        return temp + path;
                    }
                }
            }
        }

        return "";
    }

}

public interface IPropertyExpressionToken 
{
    string Name { get; }
}

public abstract class PropertyExpressionTokenBase<TInput, TOutput> : IPropertyExpressionToken
{
    public abstract string Name { get; }
    public abstract Expression<Func<TInput, TOutput>> Expression { get; }
}

public class PropertyExpressionToken<TInput, TOutput> : PropertyExpressionTokenBase<TInput, TOutput>
{
    public override Expression<Func<TInput, TOutput>> Expression { get; }
    public override string Name { get; }

    public PropertyExpressionToken(
        Expression<Func<TInput, TOutput>> expression)
    {
        Expression = expression;
        Name = ExpressionUtilities.GetPropertyPath(expression);
    }

    public PropertyExpressionToken(
        Expression<Func<TInput, TOutput>> expression,
        string name)
    {
        Expression = expression;
        Name = name;
    }
}

public class IndexedPropertyExpressionToken<TInput,TOutput> : PropertyExpressionTokenBase<TInput, TOutput>
{
    public override Expression<Func<TInput, TOutput>> Expression { get; }
    public override string Name { get; }

    public IndexedPropertyExpressionToken(
        Expression<Func<TInput, IEnumerable<TOutput>>> expression, 
        string name,
        int index)
    {
        Expression = (TInput input) => expression.Compile().Invoke(input).ElementAt(index);
        Name = name;
        Index = index;
    }

    public int Index { get; }
}

#region ScopeData
public interface IScopedData
{
    object? GetValue();
    string Describe();
}

public interface IScopedData<TResponse> : IScopedData
{
}

public class ScopedData<TPassThrough, TResponse> : IScopedData<TResponse>
{
    private Func<TPassThrough, TResponse> PassThroughFunction { get; }
    private string Description { get; }
    private TPassThrough? Result { get; }

    public ScopedData(
        string description,
        Func<TPassThrough, TResponse> passThroughFunction,
        TPassThrough? result
    )
    {
        Description = description;
        PassThroughFunction = passThroughFunction;
        Result = result;
    }

    public object? GetValue()
    {
        if (Result == null) return null;

        return PassThroughFunction.Invoke(Result);
    }

    public string Describe()
    {
        return Description;
    }
}
#endregion

#region Validators
public interface IValidationAction 
{
    ValidationActionResult Validate(object? value);
}

public interface IValidationComponent
{
    string Name { get; }
    string DescriptionTemplate { get; }
    string ErrorTemplate { get; }

    void SetDescriptionTemplate(string desc);
    void SetErrorTemplate(string message);

    IValidationAction GetValidationAction();
}

public class ValidationActionResult
{
    public ValidationActionResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }
}

public abstract class ValidationComponentBase : IValidationComponent, IValidationAction
{
    public abstract string Name { get; }
    public abstract string DescriptionTemplate { get; protected set; }
    public abstract string ErrorTemplate { get; protected set; }
    //public IParentScope CurrentParent { get; }

    public void SetDescriptionTemplate(string desc) { DescriptionTemplate = desc; }
    public void SetErrorTemplate(string error) { ErrorTemplate = error; }

    public ValidationComponentBase()
    {
        //CurrentParent = currentParent;
    }

    public IValidationAction GetValidationAction()
    {
        return this;
    }

    public ValidationActionResult Validate(object? value)
    {
        return new ValidationActionResult(
            success: true,
            message: ErrorTemplate
        );
    }
}

public class IsEqualToValidation : ValidationComponentBase //: ValidationBase<IComparable>
{
    private const string ValueToken = "value";
    public override string Name { get; } = "Equal To";
    public override string DescriptionTemplate { get; protected set; } = $"Must equal to {ValueToken}";
    public override string ErrorTemplate { get; protected set; } = $"Is not equal to {ValueToken}";
    public IComparable? Value { get; }

    public IsEqualToValidation(IComparable value)
    {
        Value = value;
    }
}

public class NotNullValidation : ValidationComponentBase
{
    public override string Name { get; } = "Not Null";
    public override string DescriptionTemplate { get; protected set; } = $"Must not be null";
    public override string ErrorTemplate { get; protected set; } = $"Is no null";

    public NotNullValidation()
    {}
}

public class CustomFuncValidation<TFieldType> : ValidationComponentBase //: ValidationBase<IComparable>
{
    public override string Name { get; } = "Custom Function";
    public override string DescriptionTemplate { get; protected set; } = $"Must Pass a custom function.";
    public override string ErrorTemplate { get; protected set; } = $"Must Pass a custom function.";
    public Func<TFieldType, bool> CustomValidation { get; }

    public CustomFuncValidation(
        Func<TFieldType, bool> customValidation,
        string description,
        string errorMessage
    )
    {
        CustomValidation = customValidation;
        DescriptionTemplate = description;
        ErrorTemplate = errorMessage;
    }
}
#endregion

#region Scopes

public interface IParentScope 
{
    Guid Id { get; }
    IParentScope? Parent { get; }
    string Name { get; }
}

public interface IValidatorScope : /*IValidatableEntity,*/ IParentScope
{
    void ExpandToValidate(ValidationConstructionStore store, object? value);
}

#region When

public abstract class ScopeBase : IValidatorScope, IExpandableEntity
{
    protected readonly ValidationConstructionStore validatorStore;
    public Guid Id { get; } = Guid.NewGuid();
    public IParentScope? Parent => this;
    public abstract string Name { get; }
    public Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? Decorator { get; protected set; } = null;

    public ScopeBase(
        ValidationConstructionStore validatorStore
    )
    {
        this.validatorStore = validatorStore;
        //Parent = parent;
        //Decorator = (item) => new WhenValidationItemDecorator(false, item.FieldDescriptor, this, item);
    }

    public virtual void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        //validatorStore.PushDecorator((item) => new WhenValidationItemDecorator(false, item.FieldDescriptor, this, item));
        //validatorStore.PushParent(this);
        InvokeScopeContainer(store, value);
        //validatorStore.PopParent();
        //validatorStore.PopDecorator();
    }

    protected abstract void InvokeScopeContainer(ValidationConstructionStore store, object? value);
}

public sealed class WhenStringValidator<TValidationType> : ScopeBase
{
    private readonly string whenDescription;
    private readonly Func<TValidationType, bool> ifTrue;
    private readonly Action rules;
    public override string Name => whenDescription;
    
    public WhenStringValidator(
        ValidationConstructionStore validatorStore,
        //IParentScope? parent,
        string whenDescription,
        Func<TValidationType, bool> ifTrue,
        Action rules
    ) : base(validatorStore)
    {
        this.whenDescription = whenDescription;
        this.ifTrue = ifTrue;
        this.rules = rules;
        Decorator = (item, parent) => new WhenValidationItemDecorator(false, item.FieldDescriptor, parent, item);
    }

    //public override void ExpandToValidate(ValidationConstructionStore store, object? value)
    //{
    //    //validatorStore.PushDecorator((item) => new WhenValidationItemDecorator(false, item.FieldDescriptor, this, item));
    //    //validatorStore.PushParent(this);
    //    InvokeScopeContainer(value);
    //    //validatorStore.PopParent();
    //    //validatorStore.PopDecorator();
    //}

    protected override void InvokeScopeContainer(ValidationConstructionStore store,object? value) { 
        rules.Invoke(); 
    }
}

public class WhenValidationItemDecorator : ValidatableStoreItem
{
    public WhenValidationItemDecorator(
        bool isVital, 
        IFieldDescriptorOutline fieldDescriptorOutline, 
        ScopeParent parent, 
        IValidatableStoreItem itemToDecorate
    )
        : base(isVital, fieldDescriptorOutline, parent, itemToDecorate)
    {
    }
}

public class PassThroughValue<TPassThrough> { }

public sealed class WhenScopedResultValidator<TValidationType, TPassThrough> : ScopeBase//IValidatorScope, IValidationScopeStoreItem
{
    private readonly string whenDescription;
    private readonly Func<TValidationType, bool> ifTrue;
    private readonly Func<TValidationType, TPassThrough> scoped;
    private readonly Action<PassThroughValue<TPassThrough>> rules;
    public override string Name => whenDescription;

    public WhenScopedResultValidator(
        //ValidationConstructionStore parentStore,
        ValidationConstructionStore validatorStore,
        //IParentScope? parent,
        string whenDescription,
        Func<TValidationType, bool> ifTrue,
        Func<TValidationType, TPassThrough> scoped,
        Action<PassThroughValue<TPassThrough>> rules
    ): base(validatorStore)
    {
        this.whenDescription = whenDescription;
        this.ifTrue = ifTrue;
        this.scoped = scoped;
        this.rules = rules;
    }

    protected override void InvokeScopeContainer(ValidationConstructionStore store, object? value)
    {
        rules.Invoke(new PassThroughValue<TPassThrough>());
    }
}

public sealed class WhenScopeToValidator<TValidationType, TPassThrough> 
    : ScopeBase, IValidatorScope, IExpandableEntity
{
    private readonly string whenDescription;
    private readonly Func<TValidationType, TPassThrough> scoped;
    private readonly Func<TValidationType, TPassThrough, bool> ifTrue;
    private readonly Action<PassThroughValue<TPassThrough>> rules;

    public override string Name => whenDescription;

    public WhenScopeToValidator(
        //ValidationConstructionStore parentStore,
        ValidationConstructionStore validatorStore,
        string whenDescription,
        Func<TValidationType, TPassThrough> scoped,
        Func<TValidationType, TPassThrough, bool> ifTrue,
        Action<PassThroughValue<TPassThrough>> rules
    ) : base(validatorStore)
    {
        //Store= parentStore;
        //Parent = parent;
        this.whenDescription = whenDescription;
        this.ifTrue = ifTrue;
        this.scoped = scoped;
        this.rules = rules;
    }

    //public override void ExpandToValidate(ValidationConstructionStore store, object? value)
    //{
    //    //validatorStore.PushDecorator((item) => new WhenValidationItemDecorator(false, item.FieldDescriptor, this, item));
    //    //validatorStore.PushParent(this);
    //    InvokeScopeContainer(value);
    //    //validatorStore.PopParent();
    //    //validatorStore.PopDecorator();
    //}

    protected override void InvokeScopeContainer(ValidationConstructionStore store, object? value)
    {
        rules.Invoke(new PassThroughValue<TPassThrough>());
    }
}

#endregion

#endregion

#region FieldDescriptor
public class SingleValidationWrapper
{
    public bool IsVital { get; }
    public IValidationComponent ValidationComponent { get; }

    public SingleValidationWrapper(IValidationComponent validationComponent)
    {
        ValidationComponent = validationComponent;
    }
}

public class FieldDescriptorMomento
{
    public IValidatorScope ParentScope { get; }
    public string PropertyName { get; }
    public Type PropertyOwner { get; }
    public Type PropertyType { get; }
    public List<SingleValidationWrapper> validationWrappers { get; } = new();

    public FieldDescriptorMomento(
        IValidatorScope parentScope,
        string propertyName,
        Type propertyType,
        Type propertyOwner)
    {
        ParentScope = parentScope;
        PropertyName = propertyName;
        PropertyType = propertyType;
        PropertyOwner = propertyOwner;
    }
}

public interface IFieldDescriptorOutline
{
    string PropertyName { get; }
    Type PropertyOwner { get; }
    Type PropertyType { get; }
}

public interface IFieldDescriptor<TValidationType, TFieldType> : IFieldDescriptorOutline
{
    PropertyExpressionTokenBase<TValidationType, TFieldType> PropertyToken { get; }
    ValidationConstructionStore Store { get; }
    //IParentScope ParentScope { get; }
    void AddValidation(IExpandableEntity component);
    
    void NextValidationIsVital();
    void AddValidation(IValidationComponent validation);
}

public class FieldDescriptionExpandableWrapper<TValidationType, TFieldType> 
    : IExpandableEntity
{
    private readonly IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor;
    private IExpandableEntity component;
    public Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? Decorator => null;
    //public IParentScope Parent => component as IParentScope;


    public FieldDescriptionExpandableWrapper(
        IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
        bool isVital, 
        IExpandableEntity component
    )
    {
        this.fieldDescriptor = fieldDescriptor;
        IsVital = isVital;
        this.component = component;
    }

    public bool IsVital { get; }

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        TFieldType? temp = default;
        if (value is TValidationType result && result != null){
            temp = fieldDescriptor.PropertyToken.Expression.Compile().Invoke(result);
        }
        component.ExpandToValidate(store, temp);
    }
}

//public class ValidatorClassExpandableWrapper<TValidationType, TFieldType> : IExpandableEntity
//{
//    private readonly IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor;
//    private ISubValidatorClass validatorClass;

//    public ValidatorClassExpandableWrapper(
//        IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
//        bool isVital,
//        ISubValidatorClass validatorClass
//    )
//    {
//        this.fieldDescriptor = fieldDescriptor;
//        IsVital = isVital;
//        this.validatorClass = validatorClass;
//    }

//    public bool IsVital { get; }

//    public void ExpandToValidate(ValidationConstructionStore store, object? value)
//    {
//        TFieldType? temp = default;
//        if (value is TValidationType result && result != null)
//        {
//            temp = fieldDescriptor.PropertyToken.Expression.Compile().Invoke(result);
//        }
//        validatorClass.ExpandToValidate(store, temp);
//    }
//}

public class FieldDescriptionNonExpandableWrapper : IExpandableEntity
{
    private readonly IFieldDescriptorOutline fieldDescriptorOutline;
    //private readonly IParentScope parentScope;
    private IValidationComponent component;
    public Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? Decorator => null;
    //public IParentScope Parent => parentScope;

    public FieldDescriptionNonExpandableWrapper(
        IFieldDescriptorOutline fieldDescriptorOutline,
        //IParentScope parentScope,
        bool isVital, 
        IValidationComponent component)
    {
        this.fieldDescriptorOutline = fieldDescriptorOutline;
        //this.parentScope = parentScope;
        IsVital = isVital;
        this.component = component;
    }

    public bool IsVital { get; }

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        //store.AddItem(
        store.AddItem(
            IsVital, 
            fieldDescriptorOutline, 
            component
            //new ValidatableStoreItem(IsVital, fieldDescriptorOutline, parentScope, component)
        );
    }
}

public class FieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType>
{
    public PropertyExpressionTokenBase<TValidationType, TFieldType> PropertyToken { get; }

    public ValidationConstructionStore Store { get; }
    //public IParentScope ParentScope { get; }
    public string PropertyName => PropertyToken.Name;
    public Type PropertyOwner => typeof(TValidationType);
    public Type PropertyType => typeof(TFieldType);

    protected bool _NextValidationVital{ get; set; } = false;

    public void NextValidationIsVital()
    {
        _NextValidationVital = true;
    }
    
    public FieldDescriptor(
        PropertyExpressionTokenBase<TValidationType, TFieldType> propertyToken,
        //IParentScope parentScope,
        ValidationConstructionStore store) 
    {
        this.PropertyToken = propertyToken;
        //ParentScope = parentScope;
        Store = store;
    }

    public void AddValidation(IExpandableEntity component)
    {
        Store.AddItem(//component);
                      //Store.GetCurrentScopeParent(), 
            new FieldDescriptionExpandableWrapper<TValidationType, TFieldType>(
                this,
                _NextValidationVital,
                component)
        );
        _NextValidationVital = false;
    }

    public void AddValidation(IValidationComponent validation)
    {
        Store.AddItem(_NextValidationVital, this, validation);

        //Store.AddItem( //_NextValidationVital, this, validation);
        //    //Store.GetCurrentScopeParent(), 
        //    new FieldDescriptionNonExpandableWrapper(
        //        this, 
        //        //ParentScope, 
        //        _NextValidationVital, 
        //        validation)
        //);
        _NextValidationVital = false;
    }
}
#endregion

public class ValidationOutcome
{
    private readonly bool succeeded;
    public bool IsFailed => !succeeded;
    public bool IsSucceeded => succeeded;
    public string PropertyName { get; set; }
    public string? Message { get; }
    public ValidationGroupResult? Group { get; set; }

    public ValidationOutcome(string propertyName, bool succeeded, string? message)
    {
        PropertyName = propertyName;
        this.succeeded = succeeded;
        Message = message;
    }
}

public interface IValidatorClass: IParentScope { }

public interface IMainValidator<TValidationType> : IValidatorClass
{
    ValidationConstructionStore Store { get; }
    //void ExpandToValidate(ValidationExecutionStore store, TValidationType instance);
}

public interface ISubValidatorClass : IParentScope, IExpandableEntity
{
}

public class ValidationOptions
{
    private IValidationComponent validationComponent;

    public ValidationOptions(IValidationComponent validationComponent)
    {
        this.validationComponent = validationComponent;
    }

    public ValidationOptions WithDescription(string desc)
    {
        validationComponent.SetDescriptionTemplate(desc);
        return this;
    }

    public ValidationOptions WithErrorMessage(string error)
    {
        validationComponent.SetErrorTemplate(error);
        return this;
    }
}

public class ValidationOptions<TCustom> : ValidationOptions
{
    public TCustom CustomOptions { get; }

    public ValidationOptions(IValidationComponent validationComponent, TCustom customOptions) 
        : base(validationComponent)
    {
        CustomOptions = customOptions;
    }
}

public static partial class IFieldValidatorExtensions
{
    public static IFieldDescriptor<TValidationType, TFieldType> Vitally<TValidationType, TFieldType>(this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor)
    {
        fieldDescriptor.NextValidationIsVital();
        return fieldDescriptor;
    }

    public static IFieldDescriptor<TValidationType, TFieldType> NotNull<TValidationType, TFieldType>(
    this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
    Action<ValidationOptions>? optionsAction = null
    )
    {
        var validation = new NotNullValidation();
        optionsAction?.Invoke(new ValidationOptions(validation));
        fieldDescriptor.AddValidation(validation);
        return fieldDescriptor;
    }

    public static IFieldDescriptor<TValidationType, TFieldType> IsEqualTo<TValidationType, TFieldType>(
        this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor, 
        TFieldType value,
        Action<ValidationOptions>? optionsAction = null
        )
        where TFieldType : IComparable
    {
        var validation = new IsEqualToValidation(value);
        optionsAction?.Invoke(new ValidationOptions(validation));
        fieldDescriptor.AddValidation(validation);
        return fieldDescriptor;
    }

    public static IFieldDescriptor<TValidationType, TFieldType> Is<TValidationType, TFieldType>(
        this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
        Func<TFieldType, bool> testFunc,
        string description,
        string errorMessage
        )
    {
        var validation = new CustomFuncValidation<TFieldType>(testFunc, description, errorMessage);
        fieldDescriptor.AddValidation(validation);
        return fieldDescriptor;
    }

    public static IFieldDescriptor<TValidationType, TFieldType> SetValidator<TValidationType, TFieldType>(
        this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
        ISubValidatorClass validatorClass
    )
    {
        fieldDescriptor.AddValidation(validatorClass);
        return fieldDescriptor;
    }
}

public abstract class AbstractValidatorBase <TValidationType> 
    : IParentScope
{
    public IParentScope? Parent { get; }
    public abstract string Name { get; }
    public Guid Id { get; } = Guid.NewGuid();

    protected AbstractValidatorBase(IParentScope? parent, ValidationConstructionStore store)
    {
        Parent = parent;
        Store = store;
        //Store.PushParent(this);
    }

    protected IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> DescribeEnumerable<TFieldType>(
        Expression<Func<TValidationType, IEnumerable<TFieldType>>> expr
    )
    {
        var fieldDescriptor = new FieldDescriptor<TValidationType, IEnumerable<TFieldType>>(
            new PropertyExpressionToken<TValidationType, IEnumerable<TFieldType>>(expr),
            //this,
            //Store.GetCurrentScopeParent(), 
            Store
        );
        return fieldDescriptor;
    }

    public IFieldDescriptor<TValidationType,TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
    {
        var fieldDescriptor = new FieldDescriptor<TValidationType, TFieldType>(
            new PropertyExpressionToken<TValidationType, TFieldType>(expr),
            //this, 
            Store
        );
        return fieldDescriptor;
    }

    public void When(
        string whenDescription,
        Func<TValidationType, bool> ifTrue,
        Action rules)
    {
        var context = new WhenStringValidator<TValidationType>(
            Store, 
            whenDescription, 
            ifTrue, 
            rules);
        Store.AddItem(context);
    }

    public void ScopeWhen<TPassThrough>(
        string whenDescription,
        Func<TValidationType, bool> ifTrue,
        Func<TValidationType, TPassThrough> scoped,
        Action<PassThroughValue<TPassThrough>> rules)
    {
        var context = new WhenScopedResultValidator<TValidationType, TPassThrough>(
            Store, 
            whenDescription, 
            ifTrue, 
            scoped, 
            rules);
        Store.AddItem(context);
    }

    public void ScopeWhen<TPassThrough>(
        string whenDescription,
        Func<TValidationType, TPassThrough> scoped,
        Func<TValidationType, TPassThrough, bool> ifTrue,
        Action<PassThroughValue<TPassThrough>> rules)
    {
        var context = new WhenScopeToValidator<TValidationType, TPassThrough>(
            Store, 
            whenDescription, 
            scoped, 
            ifTrue, 
            rules);
        Store.AddItem(context);
    }

    public ValidationConstructionStore Store { get; }
}

public abstract class PassThroughValidator<TValidationType> : AbstractValidatorBase<TValidationType>
{
    protected PassThroughValidator(IParentScope? parent, ValidationConstructionStore store) : base(parent, store) {}
}

public abstract class AbstractValidator<TValidationType> 
    : AbstractValidatorBase<TValidationType>, IMainValidator<TValidationType>
{
    public override string Name => typeof(TValidationType).Name;
    protected AbstractValidator(IParentScope? parent = null) : base(parent,new()) {}
}

public abstract class AbstractSubValidator<TValidationType>
    : AbstractValidatorBase<TValidationType>, ISubValidatorClass
{
    public override string Name => typeof(TValidationType).Name;
    public Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? Decorator => null;

    protected AbstractSubValidator() : base(null, new()) { }

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        //store.PushDecorator((item) => new WhenValidationItemDecorator(false, item.FieldDescriptor, this, item));
        //store.PushParent(this);
        //InvokeScopeContainer(value);
        //store.Merge(Store);
        var expandedItems = Store.ExpandToValidate(value);
        foreach (var item in expandedItems)
        {
            if (item != null)
                store.AddItemToCurrentScope(item);
        }
        //store.PopParent();
        //store.PopDecorator();
    }
}

public class Song
{
    public int TrackNumber { get; set; } = 0;
    public string TrackName { get; set; } = "A Song Title";
    public double Duration { get; set; } = 2.3;
    public string Description { get; set; } = "This is a description for the song.";
}

public class SongValidator : AbstractSubValidator<Song>
{
    public SongValidator()
    {
        Describe(x => x.TrackName)
            .Vitally().IsEqualTo("A Song Title2");

        When(
            "TrackNumber is 1",
            (instance) => instance.TrackNumber == 1,
            () =>
            {
                Describe(x => x.Duration)
                    .IsEqualTo(
                        2.3,
                        options =>
                            options
                                .WithErrorMessage("Duration Error Message.")
                                .WithDescription("Duration Description.")
                    );

                When(
                   "TrackNumber is not 100",
                   (instance) => instance.TrackNumber != 100,
                   () =>
                   {
                       Describe(x => x.Duration)
                        .IsEqualTo(3.3);
                           //.Is(AValidDescription,
                           //    "Should have a valid description",
                           //    "The description is invalid");
                   });
            });
    }

    private bool AValidDescription(double description) { return description == 2.3; }
}

public class Album
{
    public string Name { get; set; }
    public List<Song> Songs { get; set; } = new();

    public Album()
    {
        Name = "Album 1";
        Songs.Add(new Song());
    }
}

public static partial class IFieldValidatorExtensions
{
    public static IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> ForEach<TClassType, TPropertyType>(
            this IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> fieldDescriptor,
            Action<IFieldDescriptor<TClassType, TPropertyType>> actions
            )
    {
        var forEachScope = new ForEachScope<TClassType, TPropertyType>(
            fieldDescriptor,
            actions
        );
        fieldDescriptor.AddValidation(forEachScope);
        return fieldDescriptor;
    }
}

public class AlbumValidator : AbstractValidator<Album>
{
    public AlbumValidator()
    {
        Describe(x => x.Name)
        .NotNull();

        When("Soemthing", (x) => true, () =>
        {
            //Describe(x => x.Name)
            //    .NotNull();
            When("another somethign", (x) => true, () =>
            {
                DescribeEnumerable(x => x.Songs)
                    .Vitally().NotNull()
                    .ForEach(x => x
                        .Vitally().NotNull()
                        .SetValidator(new SongValidator())
                    )
                    ;
            });
        });
    }
}

public class ValidationItemResult 
{
    public string Property { get; }
    public List<ValidationOutcome> Outcomes { get; } = new();
    

    public ValidationItemResult(string property)
    {
        Property = property;
    }
}

public class ValidationGroupResult 
{
    //[JsonIgnore]
    public Guid Id { get; }
    [JsonIgnore]
    public IParentScope ParentScope { get; }
    public string Name = "";
    public string Description = "";
    public ValidationGroupResult? Group;

    public ValidationGroupResult(IParentScope parentScope, ValidationGroupResult? group)
    {
        Id = parentScope.Id;
        ParentScope = parentScope;
        Name = parentScope.Name;
        Description = parentScope.Name;
        Group = group;
    }
}

public class ValidationResult 
{
    public List<ValidationItemResult> Results = new();

    public void AddItem(IValidatableStoreItem item, ValidationActionResult outcome)
    {
        var scopes = new List<IParentScope>();

        //if( item.ScopeParent?.CurrentScope is not null)
        //    scopes.Add(item.ScopeParent.CurrentScope);

        var child = item.ScopeParent;
        while (child != null)
        {
            if (child.CurrentScope != null) 
            {
                scopes.Add(child.CurrentScope);
            }
            child = child.PreviousScope;
        }
        
        var existingProperty = Results.SingleOrDefault(r => r.Property == item.FieldDescriptor.PropertyName);
        var newOutcome = new ValidationOutcome(item.FieldDescriptor.PropertyName, outcome.Success, outcome.Message);
        if (existingProperty != null)
        {
            existingProperty.Outcomes.Add(newOutcome);
        }
        else
        {
            existingProperty = new ValidationItemResult(item.FieldDescriptor.PropertyName);
            existingProperty.Outcomes.Add(newOutcome);

            Results.Add(existingProperty);
        }

        if (scopes.Count > 0)
        {
            ValidationGroupResult? group = null;
            foreach(var scope in scopes)
            {
                group = new ValidationGroupResult(scope, group);
            }
            newOutcome.Group = group;
        }
    }
}

public class DescriptionResult { }

public class ValidationRunner<TValidationType>
{
    private readonly IEnumerable<IMainValidator<TValidationType>> mainValidators;

    public ValidationRunner(IEnumerable<IMainValidator<TValidationType>> mainValidators)
    {
        this.mainValidators = mainValidators;
    }

    public ValidationResult Validate(TValidationType instance)
    {
        var validationResult = new ValidationResult();
        var store = new ValidationConstructionStore();
        var executionStore = new ValidationExecutionStore();
        var allItems = new List<IValidatableStoreItem>();

        foreach (var mainValidator in mainValidators)
        {
            var expandedItems = mainValidator.Store.ExpandToValidate(instance);
            if (expandedItems?.Any() == true)
            {
                allItems.AddRange(expandedItems);
            }
        }

        var groupedItems = allItems
            .GroupBy(x => new { x.FieldDescriptor.PropertyOwner.Name, x.FieldDescriptor.PropertyName });

        foreach (var validationObjectGroup in groupedItems)
        {
            foreach(var propertyGroup in validationObjectGroup)
            {
                var result = propertyGroup.GetValidationAction().Validate(null);
                //if (!result.Success)
                {
                    validationResult.AddItem(propertyGroup, result);// propertyGroup.ParentScope, result.Result);

                    if (propertyGroup.IsVital) break;
                }
            }
        }

        return validationResult;
    }

    public DescriptionResult Describe() {

        return new DescriptionResult();
    }
}

public class BasicLevelTests
{
    [Fact]
    public void BasicValidator_Describe_ToJson()
    {
        // Arrange
        var validator = new AlbumValidator();
        var runner = new ValidationRunner<Album>(new List<IMainValidator<Album>>() { validator });

        // Act
        var results = runner.Describe();
        var json = new JsonConverter().ToJson(results);

        // Assert
        Approvals.VerifyJson(json);
    }

    [Fact]
    public void BasicValidator_Validate_ToJson()
    {
        // Arrange
        var validator = new AlbumValidator();
        var runner = new ValidationRunner<Album>(new List<IMainValidator<Album>>() { validator });

        // Act
        var results = runner.Validate(new Album());
        var json = new JsonConverter().ToJson(results);

        // Assert
        Approvals.VerifyJson(json);
    }
}