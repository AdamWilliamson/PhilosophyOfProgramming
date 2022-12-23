using ApprovalTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

    public virtual string CombineWithParentProperty(string parentProperty)
    {
        return parentProperty + "." + Name;
    }
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

public class IndexedPropertyExpressionToken<TInput, TOutput> 
    : PropertyExpressionTokenBase<TInput, TOutput>
    where TInput: IEnumerable<TOutput>
{
    public override Expression<Func<TInput, TOutput>> Expression { get; }
    public override string Name { get; }

    public IndexedPropertyExpressionToken(
        //Expression<Func<TInput, TOutput>> expression, 
        string name,
        int index)
    {
        Expression = (TInput input) => input.ElementAt(index);
        Name = name;
        Index = index;
    }

    public int Index { get; }

    public override string CombineWithParentProperty(string parentProperty)
    {
        if (Index < 0) return parentProperty + $"[n]";
        return parentProperty + $"[{Index}]";
    }
}

#region ScopeData

public interface IScopeData
{
    void ReHome(IFieldDescriptorOutline fieldDescriptorOutline);
    Task Init(object? instance);
    object? GetValue();
    void SetParent(IScopeData parent);
}

public interface IScopeData<TResponse> : IScopeData { }

public class ScopedData<TPassThrough, TResponse> : IScopeData
    //where TPassThrough : class
{
    private Func<TPassThrough, Task<TResponse>> PassThroughFunction { get; }
    protected IScopeData Parent { get; set; }
    TResponse RetrievedValue;

    public ScopedData(Func<TPassThrough, Task<TResponse>> passThroughFunction)
    {
        PassThroughFunction = passThroughFunction;
    }

    public ScopedData(IScopeData parent, Func<TPassThrough, Task<TResponse>> passThroughFunction)
    {
        Parent = parent;
        PassThroughFunction = passThroughFunction;
    }

    public void SetParent(IScopeData parent)
    {
        if (Parent == null)
        {
            Parent = parent;
        }
        else
        {
            Parent.SetParent(parent);
        }
    }

    public void ReHome(IFieldDescriptorOutline fieldDescriptorOutline)
    {
        if (fieldDescriptorOutline == null) return;

        SetParent(
            new ScopedData<object?, Task<TPassThrough>>(
                async (object? instance) => 
                    Task.FromResult((TPassThrough)fieldDescriptorOutline.GetValue(instance))
            )
        );
    }

    public async Task Init(object? instance)
    {
        if (Parent != null)
        {
            await Parent.Init(instance);
            var parentValue = Parent.GetValue();
            RetrievedValue = await PassThroughFunction.Invoke((TPassThrough)parentValue);
        }

        RetrievedValue =  await PassThroughFunction.Invoke((TPassThrough)instance);
    }

    public object? GetValue()
    {
        return RetrievedValue;
    }

    // scopeData.To(x => x.Y);
    public ScopedData<TPassThrough, TNewResponse> To<TNewResponse>(
        Func<TPassThrough, Task<TNewResponse>> passThroughFunction
    )
    {
        return new ScopedData<TPassThrough, TNewResponse>(this, passThroughFunction);
    }
}
#endregion

#region Validators
//public interface IValidationAction 
//{
//    ValidationActionResult Validate(object? value);
//    DescribeActionResult Describe();
//}

public interface IValidationComponent
{
    string Name { get; }
    string DescriptionTemplate { get; }
    string ErrorTemplate { get; }

    void SetDescriptionTemplate(string desc);
    void SetErrorTemplate(string message);

    //IValidationAction GetValidationAction();
    Task InitScopes(object? instance);
    ValidationActionResult Validate(object? value);
    DescribeActionResult Describe();
    void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor);
}

public class ValidationActionResult
{
    public ValidationActionResult(
        string validator,
        bool success, 
        string message,
        List<KeyValuePair<string,string>> keyValues)
    {
        Validator = validator;
        Success = success;
        Message = message;
        KeyValues = keyValues;
    }

    public static ValidationActionResult Successful(string validator)
    {
        return new ValidationActionResult(validator, true, "", new());
    }

    public string Validator { get; }
    public bool Success { get; }
    public string Message { get; protected set; }
    public List<KeyValuePair<string, string>> KeyValues { get; }

    public virtual List<string>? GetFailedDependantFields(string currentProperty, ValidationResult currentValidationResult)
    {
        return null;
    }

    public void UpdateMessageProcessor(string message)
    {
        Message = message;
    }
}

public class DescribeActionResult
{
    public DescribeActionResult(
        string validator,
        string message,
        List<KeyValuePair<string, string>> keyValues)
    {
        Validator = validator;
        Message = message;
        KeyValues = keyValues;
    }

    public string Validator { get; }
    public string Message { get; protected set; }
    public List<KeyValuePair<string, string>> KeyValues { get; }

    internal void UpdateDescription(string message)
    {
        Message = message;
    }
}

public abstract class ValidationComponentBase : IValidationComponent
{
    public abstract string Name { get; }
    public abstract string DescriptionTemplate { get; protected set; }
    public abstract string ErrorTemplate { get; protected set; }

    public virtual void SetDescriptionTemplate(string desc) { DescriptionTemplate = desc; }
    public virtual void SetErrorTemplate(string error) { ErrorTemplate = error; }

    public ValidationComponentBase(){}
   
    public virtual void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor) { }
    public virtual Task InitScopes(object? instance) { return Task.CompletedTask; }
    public abstract ValidationActionResult Validate(object? value);

    public abstract DescribeActionResult Describe();
}

public class IsEqualToValidation : ValidationComponentBase
{
    private const string ValueToken = "value";
    private readonly IComparable? Value = null;
    private readonly IScopeData? scopedData = null;

    public IsEqualToValidation(IScopeData scopedData)
    {
        this.scopedData = scopedData;
    }

    public IsEqualToValidation(IComparable? value)
    {
        Value = value;
    }

    public override string Name { get; } = "Equal To";
    public override string DescriptionTemplate { get; protected set; } = $"Must equal to '{ValueToken}'";
    public override string ErrorTemplate { get; protected set; } = $"Is not equal to '{ValueToken}'";

    public override void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor)
    {
        scopedData?.ReHome(attemptedScopeFieldDescriptor);
    }

    protected TResult? GetData<TResult>(IScopeData? scopedData, TResult? value) 
    {
        return scopedData switch
        {
            null => value,
            _ => (TResult?)scopedData.GetValue()
        };
    }

    public override ValidationActionResult Validate(object? value)
    {
        Debug.WriteLine("IsEqualTo - Validate: " + value?.ToString());
        var RealValue = GetData(scopedData, Value);

        if (
            (RealValue == null && value == null)
            || RealValue?.Equals(value) == true
        )
        {
            return ValidationActionResult.Successful(nameof(IsEqualToValidation));
        }
        else
        {
            var valueAsString = RealValue?.ToString() ?? "null";

            return new ValidationActionResult(
                validator: nameof(IsEqualToValidation),
                success: false,
                message: ErrorTemplate,
                new List<KeyValuePair<string, string>>()
                {
                    new(ValueToken, valueAsString)
                }
            );
        }
    }

    public override DescribeActionResult Describe()
    {
        var RealValue = GetData(scopedData, Value);
        var valueAsString = RealValue?.ToString() ?? "null";

        return new DescribeActionResult(
            validator: nameof(IsEqualToValidation),
            message: DescriptionTemplate,
            new List<KeyValuePair<string, string>>()
            {
                new(ValueToken, valueAsString)
            }
        );
    }
}

public class NotNullValidation : ValidationComponentBase
{
    public override string Name { get; } = "Not Null";
    public override string DescriptionTemplate { get; protected set; } = $"Must not be null";
    public override string ErrorTemplate { get; protected set; } = $"Is no null";

    public NotNullValidation(){}

    public override ValidationActionResult Validate(object? value)
    {
        Debug.WriteLine("NotNull - Validate: " + value?.ToString());

        if (value != null)
        {
            return ValidationActionResult.Successful(nameof(NotNullValidation));
        }
        else
        {
            return new ValidationActionResult(
               validator: nameof(NotNullValidation),
                success: false,
                message: ErrorTemplate,
                new List<KeyValuePair<string, string>>()
            );
        }
    }

    public override DescribeActionResult Describe()
    {
        return new DescribeActionResult(
            validator: nameof(NotNullValidation),
            message: DescriptionTemplate,
            new List<KeyValuePair<string, string>>()
        );
    }
}

public class CustomFuncValidation<TFieldType> : ValidationComponentBase
{
    public override string Name { get; } = "Custom Function";
    public override string DescriptionTemplate { get; protected set; } = $"Must Pass a custom function.";
    public override string ErrorTemplate { get; protected set; } = $"Must Pass a custom function.";
    public Func<TFieldType, ValidationActionResult> CustomValidation { get; }

    public CustomFuncValidation(
        Func<TFieldType, ValidationActionResult> customValidation,
        string description,
        string errorMessage
    )
    {
        CustomValidation = customValidation;
        DescriptionTemplate = description;
        ErrorTemplate = errorMessage;
    }

    public override ValidationActionResult Validate(object? value)
    {
        return CustomValidation.Invoke((TFieldType) value);
    }

    public override DescribeActionResult Describe()
    {
        return new DescribeActionResult(
            validator: nameof(CustomFuncValidation<TFieldType>),
            message: DescriptionTemplate,
            new List<KeyValuePair<string, string>>()
        );
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

public interface IValidatorScope : IParentScope
{
    void ExpandToValidate(ValidationConstructionStore store, object? value);
}

#region When

public abstract class ScopeBase : IValidatorScope, IExpandableEntity
{
    public virtual bool IgnoreScope => false;
    protected readonly ValidationConstructionStore validatorStore;
    public Guid Id { get; } = Guid.NewGuid();
    public IParentScope? Parent => this;
    public abstract string Name { get; }
    public Func<IValidatableStoreItem, IValidatableStoreItem>? Decorator { get; protected set; } = null;
    protected bool IsVital = false;
    public void AsVital() { IsVital = true; }

    public ScopeBase(
        ValidationConstructionStore validatorStore
    )
    {
        this.validatorStore = validatorStore;
        //Decorator = (item) => new WhenValidationItemDecorator(false, item.FieldDescriptor, this, item);
    }

    public virtual void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        InvokeScopeContainer(store, value);
    }

    public void ExpandToDescribe(ValidationConstructionStore store)
    {
        InvokeScopeContainerToDescribe(store);
    }

    protected abstract void InvokeScopeContainer(ValidationConstructionStore store, object? value);
    protected abstract void InvokeScopeContainerToDescribe(ValidationConstructionStore store);
}

public class WhenStringValidator_IfTrue<TValidationType>
{
    Func<TValidationType, Task<bool>> ifTrue;
    bool? result = null;

    public WhenStringValidator_IfTrue(Func<TValidationType, Task<bool>> ifTrue)
    {
        this.ifTrue = ifTrue;
    }

    public async Task<bool> CanValidate(TValidationType input)
    {
        if (result != null) return result.Value;

        return await ifTrue.Invoke(input);
    }
}

public sealed class WhenStringValidator<TValidationType> : ScopeBase
{
    private readonly string whenDescription;
    private readonly Func<TValidationType, Task<bool>> ifTrue;
    private readonly Action rules;
    public override string Name => whenDescription;
    WhenStringValidator_IfTrue<TValidationType> something;

    public WhenStringValidator(
        ValidationConstructionStore validatorStore,
        string whenDescription,
        Func<TValidationType, Task<bool>> ifTrue,
        Action rules
    ) : base(validatorStore)
    {
        this.whenDescription = whenDescription;
        this.ifTrue = ifTrue;
        this.rules = rules;
        something = new WhenStringValidator_IfTrue<TValidationType>(ifTrue);
        Decorator = (item) => new WhenValidationItemDecorator<TValidationType>(item, something, null);
    }

    protected override void InvokeScopeContainer(ValidationConstructionStore store,object? value) { 
        rules.Invoke(); 
    }

    protected override void InvokeScopeContainerToDescribe(ValidationConstructionStore store)
    {
        rules.Invoke();
    }
}

public abstract class WhenValidationItemDecoratorBase<TValidationType> : IValidatableStoreItem
{
    public WhenValidationItemDecoratorBase(
        IValidatableStoreItem itemToDecorate
    )
    {
        ItemToDecorate = itemToDecorate;
    }

    public bool IsVital => ItemToDecorate.IsVital;

    public ScopeParent? ScopeParent
    {
        get { return ItemToDecorate.ScopeParent; }
        set { ItemToDecorate.ScopeParent = value; }
    }

    public FieldExecutor CurrentFieldExecutor
    {
        get { return ItemToDecorate.CurrentFieldExecutor; }
        set { ItemToDecorate.CurrentFieldExecutor = value; }
    }

    public IFieldDescriptorOutline FieldDescriptor
    {
        get { return ItemToDecorate.FieldDescriptor; }
        set { ItemToDecorate.FieldDescriptor = value; }
    }

    public IValidationComponent Component => ItemToDecorate.Component;

    public IValidatableStoreItem ItemToDecorate { get; }

    public virtual DescribeActionResult Describe()
    {
        //var x = new DescribeActionResult("when");
        return ItemToDecorate.Describe();
    }

    public IValidatableStoreItem? GetChild()
    {
        return ItemToDecorate.GetChild();
    }

    public object? GetValue(object? value)
    {
        return ItemToDecorate.GetValue(value);
    }

    public void SetParent(FieldExecutor fieldExecutor)
    {
        ItemToDecorate.SetParent(fieldExecutor);
    }

    public abstract Task<bool> CanValidate(object? instance);

    public virtual void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor) 
    {
        ItemToDecorate.ReHomeScopes(attemptedScopeFieldDescriptor);
    }
    public Task InitScopes(object? instance)
    {
        return ItemToDecorate.InitScopes(instance);
    }
    public virtual ValidationActionResult Validate(object? value)
    {
        return ItemToDecorate.Validate(value);
    }
}

public class WhenValidationItemDecorator<TValidationType> : WhenValidationItemDecoratorBase<TValidationType>
{
    private readonly WhenStringValidator_IfTrue<TValidationType> ifTrue;
    private readonly IScopeData? scopedData;

    public WhenValidationItemDecorator(
        IValidatableStoreItem itemToDecorate,
        WhenStringValidator_IfTrue<TValidationType> ifTrue,
        IScopeData? scopedData
    ) : base(itemToDecorate)
    {
        this.ifTrue = ifTrue;
        this.scopedData = scopedData;
    }

    public override async Task<bool> CanValidate(object? instance)
    {
        //var diffValue = ItemToDecorate..CurrentFieldExecutor.GetValue(instance);
        //var value = FieldDescriptor?.GetValue(instance) ?? instance;
        var result = await ifTrue.CanValidate((TValidationType)instance);
        scopedData?.Init(instance);
        
        if (result)
            return await ItemToDecorate.CanValidate(instance);
        return result;
    }
}

public class WhenValidationItemDecorator_Scoped<TValidationType, TPassThrough> : WhenValidationItemDecoratorBase<TValidationType>
{
    private readonly WhenStringValidator_IfTruescoped<TValidationType, TPassThrough> ifTrue;

    public WhenValidationItemDecorator_Scoped(
        IValidatableStoreItem itemToDecorate,
        WhenStringValidator_IfTruescoped<TValidationType, TPassThrough> ifTrue
    ) : base(itemToDecorate)
    {
        this.ifTrue = ifTrue;
    }

    public override Task<bool> CanValidate(object? instance)
    {
        return ifTrue.CanValidate((TValidationType)instance);
    }
}

public sealed class WhenScopedResultValidator<TValidationType, TPassThrough> : ScopeBase
{
    private readonly string whenDescription;
    private readonly Func<TValidationType, Task<bool>> ifTrue;
    private readonly ScopedData<TValidationType, TPassThrough> scoped;
    private readonly Action<ScopedData<TValidationType, TPassThrough>> rules;
    public override string Name => whenDescription;
    WhenStringValidator_IfTrue<TValidationType> something;

    public WhenScopedResultValidator(
        ValidationConstructionStore validatorStore,
        string whenDescription,
        Func<TValidationType, Task<bool>> ifTrue,
        Func<TValidationType, Task<TPassThrough>> scoped,
        Action<ScopedData<TValidationType, TPassThrough>> rules
    ): base(validatorStore)
    {
        this.whenDescription = whenDescription;
        this.ifTrue = ifTrue;
        this.scoped = new ScopedData<TValidationType, TPassThrough>(scoped);
        this.rules = rules;

        something = new WhenStringValidator_IfTrue<TValidationType>(ifTrue);
        Decorator = (item) => new WhenValidationItemDecorator<TValidationType>(
            item, 
            something,
            this.scoped
            );
    }

    protected override void InvokeScopeContainer(ValidationConstructionStore store, object? value)
    {
        rules.Invoke(scoped);
    }

    protected override void InvokeScopeContainerToDescribe(ValidationConstructionStore store)
    {
        rules.Invoke(scoped);
    }
}

public class WhenStringValidator_IfTruescoped<TValidationType, TScopedData>
{
    Func<TValidationType, TScopedData, Task<bool>> ifTrue;
    private readonly ScopedData<TValidationType, TScopedData> scopedData;
    bool? result = null;

    public WhenStringValidator_IfTruescoped(
        Func<TValidationType, TScopedData, Task<bool>> ifTrue,
        ScopedData<TValidationType, TScopedData> scopedData
        )
    {
        this.ifTrue = ifTrue;
        this.scopedData = scopedData;
    }

    public async Task<bool> CanValidate(TValidationType input)
    {
        if (result != null) return result.Value;
        await scopedData.Init(input);
        var data = scopedData.GetValue();
        if (data is not TScopedData converted) return false;

        return await ifTrue.Invoke(input, converted);
    }
}

public sealed class WhenScopeToValidator<TValidationType, TPassThrough> 
    : ScopeBase, IValidatorScope, IExpandableEntity
{
    private readonly string whenDescription;
    private readonly ScopedData<TValidationType, TPassThrough> scoped;
    private readonly Action<ScopedData<TValidationType, TPassThrough>> rules;

    public override string Name => whenDescription;
    WhenStringValidator_IfTruescoped<TValidationType, TPassThrough> something;

    public WhenScopeToValidator(
        ValidationConstructionStore validatorStore,
        string whenDescription,
        Func<TValidationType, Task<TPassThrough>> scoped,
        Func<TValidationType, TPassThrough, Task<bool>> ifTrue,
        Action<ScopedData<TValidationType, TPassThrough>> rules
    ) : base(validatorStore)
    {
        this.whenDescription = whenDescription;
        this.scoped = new ScopedData<TValidationType, TPassThrough>(scoped);
        this.rules = rules;
        something = new WhenStringValidator_IfTruescoped<TValidationType, TPassThrough>
            (
            ifTrue,
            this.scoped
            );
        Decorator = (item) => new WhenValidationItemDecorator_Scoped
        <TValidationType, TPassThrough>
        (
            item, 
            something
        );
    }

    protected override void InvokeScopeContainer(ValidationConstructionStore store, object? value)
    {
        rules.Invoke(scoped);
    }

    protected override void InvokeScopeContainerToDescribe(ValidationConstructionStore store)
    {
        rules.Invoke(scoped);
    }
}

#endregion

#endregion

#region FieldDescriptor

public interface IFieldDescriptorOutline
{
    string PropertyName { get; }
    string AddTo(string existing);
    object? GetValue(object? input);
}

public interface IFieldDescriptor<TValidationType, TFieldType> : IFieldDescriptorOutline
{
    PropertyExpressionTokenBase<TValidationType, TFieldType> PropertyToken { get; }
    ValidationConstructionStore Store { get; }
    void AddValidation(IExpandableEntity component);
    
    void NextValidationIsVital();
    void AddValidation(IValidationComponent validation);
}

public class FieldDescriptionExpandableWrapper<TValidationType, TFieldType> 
    : IExpandableEntity
{
    public bool IgnoreScope => false;
    private readonly IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor;
    private IExpandableEntity component;
    public Func<IValidatableStoreItem, IValidatableStoreItem>? Decorator => null;
    object? RetrievedValue = null;
    bool ValueHasBeenRetrieved = false;

    public void AsVital() { IsVital = true; }

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

    public bool IsVital { get; protected set; }

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        if (IsVital) component.AsVital();

        component.ExpandToValidate(store, value);
    }

    public void ExpandToDescribe(ValidationConstructionStore store)
    {
        component.ExpandToDescribe(store);
    }

    public object? GetValue(object? value)
    {
        if (ValueHasBeenRetrieved) return RetrievedValue;

        TFieldType? temp = default;
        if (value is TValidationType result && result != null)
        {
            RetrievedValue = fieldDescriptor.PropertyToken.Expression.Compile().Invoke(result);
            ValueHasBeenRetrieved = true;
        }
        return RetrievedValue;
    }
}

public class FieldDescriptionNonExpandableWrapper : IExpandableEntity
{
    public bool IgnoreScope => false;
    private readonly IFieldDescriptorOutline fieldDescriptorOutline;
    private IValidationComponent component;
    public Func<IValidatableStoreItem, IValidatableStoreItem>? Decorator => null;
    public void AsVital() { IsVital = true; }

    public FieldDescriptionNonExpandableWrapper(
        IFieldDescriptorOutline fieldDescriptorOutline,
        bool isVital, 
        IValidationComponent component)
    {
        this.fieldDescriptorOutline = fieldDescriptorOutline;
        IsVital = isVital;
        this.component = component;
    }

    public bool IsVital { get; protected set; }

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        store.AddItem(
            IsVital, 
            fieldDescriptorOutline, 
            component
        );
    }

    public void ExpandToDescribe(ValidationConstructionStore store)
    {
        store.AddItem(
            IsVital,
            fieldDescriptorOutline,
            component
        );
    }
}

public class FieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType>
{
    public PropertyExpressionTokenBase<TValidationType, TFieldType> PropertyToken { get; }
    object? RetrievedValue = null;
    bool ValueHasBeenRetrieved = false;
    public ValidationConstructionStore Store { get; }
    public string PropertyName => PropertyToken.Name;
    public string AddTo(string existing)
    {
        return PropertyToken.CombineWithParentProperty(existing);
    }

    protected bool _NextValidationVital{ get; set; } = false;
    protected bool _AlwaysVital { get; set; } = false;

    public void NextValidationIsVital()
    {
        _NextValidationVital = true;
    }

    public void IsAlwaysVital()
    {
        _AlwaysVital = true;
    }

    public FieldDescriptor(
        PropertyExpressionTokenBase<TValidationType, TFieldType> propertyToken,
        ValidationConstructionStore store) 
    {
        this.PropertyToken = propertyToken;
        Store = store;
    }

    public void AddValidation(IExpandableEntity component)
    {
        Store.AddItem(
            this,
            new FieldDescriptionExpandableWrapper<TValidationType, TFieldType>(
                this,
                _NextValidationVital || _AlwaysVital,
                component)
        );
        _NextValidationVital = false;
    }

    public void AddValidation(IValidationComponent validation)
    {
        Store.AddItem(_NextValidationVital || _AlwaysVital, this, validation);
        _NextValidationVital = false;
    }

    public object? GetValue(object? value)
    {
        if (ValueHasBeenRetrieved) return RetrievedValue;

        if (value is TValidationType result && result != null)
        {
            RetrievedValue = PropertyToken.Expression.Compile().Invoke(result);
            ValueHasBeenRetrieved = true;
        }
        return RetrievedValue;
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

public class DescriptionOutcome
{
    public string PropertyName { get; set; }
    public string? Message { get; }
    public ValidationGroupResult? Group { get; set; }

    public DescriptionOutcome(string propertyName, string? message)
    {
        PropertyName = propertyName;
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

    public static IFieldDescriptor<TValidationType, TFieldType> IsEqualTo<TValidationType, TFieldType, TPassThrough>(
     this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
     ScopedData<TValidationType, TPassThrough> scopedData,
     Action<ValidationOptions>? optionsAction = null
     )
     where TPassThrough : IComparable
    {
        var validation = new IsEqualToValidation(scopedData);
        optionsAction?.Invoke(new ValidationOptions(validation));
        fieldDescriptor.AddValidation(validation);
        return fieldDescriptor;
    }

    public static IFieldDescriptor<TValidationType, TFieldType> Is<TValidationType, TFieldType>(
        this IFieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
        Func<TFieldType, ValidationActionResult> testFunc,
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
    }

    protected IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> DescribeEnumerable<TFieldType>(
        Expression<Func<TValidationType, IEnumerable<TFieldType>>> expr
    )
    {
        var fieldDescriptor = new FieldDescriptor<TValidationType, IEnumerable<TFieldType>>(
            new PropertyExpressionToken<TValidationType, IEnumerable<TFieldType>>(expr),
            Store
        );
        return fieldDescriptor;
    }

    public IFieldDescriptor<TValidationType,TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
    {
        var fieldDescriptor = new FieldDescriptor<TValidationType, TFieldType>(
            new PropertyExpressionToken<TValidationType, TFieldType>(expr),
            Store
        );
        return fieldDescriptor;
    }

    public void When(
        string whenDescription,
        Func<TValidationType, Task<bool>> ifTrue,
        Action rules)
    {
        var context = new WhenStringValidator<TValidationType>(
            Store, 
            whenDescription, 
            ifTrue, 
            rules);
        Store.AddItem(null, context);
    }

    public void ScopeWhen<TPassThrough>(
        string whenDescription,
        Func<TValidationType, Task<bool>> ifTrue,
        Func<TValidationType, Task<TPassThrough>> scoped,
        Action<ScopedData<TValidationType, TPassThrough>> rules)
    {
        var context = new WhenScopedResultValidator<TValidationType, TPassThrough>(
            Store, 
            whenDescription, 
            ifTrue, 
            scoped, 
            rules);
        Store.AddItem(null, context);
    }

    public void ScopeWhen<TPassThrough>(
        string whenDescription,
        Func<TValidationType, Task<TPassThrough>> scoped,
        Func<TValidationType, TPassThrough, Task<bool>> ifTrue,
        Action<ScopedData<TValidationType, TPassThrough>> rules)
    {
        var context = new WhenScopeToValidator<TValidationType, TPassThrough>(
            Store, 
            whenDescription, 
            scoped, 
            ifTrue, 
            rules);
        Store.AddItem(null, context);
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
    public bool IgnoreScope => false;
    public override string Name => typeof(TValidationType).Name;
    public Func<IValidatableStoreItem, IValidatableStoreItem>? Decorator => null;

    bool _isVital = false;
    public void AsVital() {  _isVital = true; }

    protected AbstractSubValidator() : base(null, new()) { }

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        var expandedItems = Store.ExpandToValidate(value);
        foreach (var item in expandedItems)
        {
            if (item != null)
                store.AddItemToCurrentScope(item);
        }
    }

    public void ExpandToDescribe(ValidationConstructionStore store)
    {
        var expandedItems = Store.ExpandToDescribe();
        foreach (var item in expandedItems)
        {
            if (item != null)
                store.AddItemToCurrentScope(item);
        }
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
            (instance) => Task.FromResult(instance.TrackNumber == 0),
            () =>
            {
                Describe(x => x.Duration)
                    .IsEqualTo(
                        3.3,
                        options =>
                            options
                                .WithErrorMessage("Duration Error Message.")
                                .WithDescription("Duration Description.")
                    );

                When(
                   "TrackNumber is not 100",
                   (instance) => Task.FromResult(instance.TrackNumber != 100),
                   () =>
                   {
                       Describe(x => x.Duration)
                            .IsEqualTo(3.3333);
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
        Songs.Add(new Song());
        Songs.Add(null);
        Songs.Add(new Song());
    }
}

public static partial class IFieldValidatorExtensions
{
    public static IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> ForEach<TClassType, TPropertyType>(
            this IFieldDescriptor<TClassType, IEnumerable<TPropertyType>> fieldDescriptor,
            Action<IFieldDescriptor<IEnumerable<TPropertyType>, TPropertyType>> actions
            )
    {
        var forEachScope = new ForEachScope<TClassType, TPropertyType>(
            fieldDescriptor,
            //null,
            actions
        );
        fieldDescriptor.AddValidation(forEachScope);
        return fieldDescriptor;
    }
}

public class RepositoryFake
{
    public static Task<string> GetStringValue(string result) { 
        return Task.FromResult(result); 
    }
}

public class AlbumValidator : AbstractValidator<Album>
{
    public AlbumValidator()
    {
        Describe(x => x.Name)
            .NotNull();

        ScopeWhen(
            "Soemthing",
            async (Album value) => await RepositoryFake.GetStringValue("Album 1!"),
            (x, scopeData) => Task.FromResult(scopeData != "Album 1"),
            (scopeData) =>
        {
            Describe(x => x.Name)
                .IsEqualTo(scopeData);
        });

        ScopeWhen(
            "Soemthing",
            (x) => Task.FromResult(true),
            async (Album value) => await RepositoryFake.GetStringValue("a string"),
            (scopeData) =>
        {
            Describe(x => x.Name)
                .IsEqualTo("Not The Name");

            When(
                "another somethign",
                (x) => Task.FromResult(true),
                () =>
            {
                DescribeEnumerable(x => x.Songs)
                    .NotNull()
                .Vitally().ForEach(x => x
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

public class DescriptionItemResult
{
    public string Property { get; }
    public List<DescriptionOutcome> Outcomes { get; } = new();


    public DescriptionItemResult(string property)
    {
        Property = property;
    }
}

public class ValidationGroupResult 
{
    [JsonIgnore]
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

    public void AddItem(ExpandedItem item, ValidationActionResult outcome)
    {
        var scopes = new List<IParentScope>();

        var child = item.ScopeParent;
        while (child != null)
        {
            if (child.CurrentScope != null
                && !scopes.Any(s => s.Id == child.CurrentScope.Id)) 
            {
                scopes.Add(child.CurrentScope);
            }
            child = child.PreviousScope;
        }
        var realName = item.FullAddressableName;

        var existingProperty = Results.SingleOrDefault(r => r.Property == item.PropertyName);
        var newOutcome = new ValidationOutcome(item.PropertyName, outcome.Success, outcome.Message);
        if (existingProperty != null)
        {
            existingProperty.Outcomes.Add(newOutcome);
        }
        else
        {
            existingProperty = new ValidationItemResult(item.PropertyName);
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

public class DescriptionResult 
{
    public List<DescriptionItemResult> Results = new();

    public void AddItem(ExpandedItem item, DescribeActionResult outcome)
    {
        var scopes = new List<IParentScope>();

        var child = item.ScopeParent;
        while (child != null)
        {
            if (child.CurrentScope != null 
                && !scopes.Any(s => s.Id == child.CurrentScope.Id))
            {
                scopes.Add(child.CurrentScope);
            }
            child = child.PreviousScope;
        }

        var existingProperty = Results.SingleOrDefault(r => r.Property == item.PropertyName);
        var newOutcome = new DescriptionOutcome(item.PropertyName, outcome.Message);
        if (existingProperty != null)
        {
            existingProperty.Outcomes.Add(newOutcome);
        }
        else
        {
            existingProperty = new DescriptionItemResult(item.PropertyName);
            existingProperty.Outcomes.Add(newOutcome);

            Results.Add(existingProperty);
        }

        if (scopes.Count > 0)
        {
            ValidationGroupResult? group = null;
            foreach (var scope in scopes)
            {
                group = new ValidationGroupResult(scope, group);
            }
            newOutcome.Group = group;
        }
    }

}

public class ValidationRunner<TValidationType>
{
    private readonly IEnumerable<IMainValidator<TValidationType>> mainValidators;
    private readonly MessageProcessor messageProcessor;

    public ValidationRunner(
        IEnumerable<IMainValidator<TValidationType>> mainValidators,
        MessageProcessor messageProcessor
        )
    {
        this.mainValidators = mainValidators;
        this.messageProcessor = messageProcessor;
    }

    public async Task<ValidationResult> Validate(TValidationType instance)
    {
        var validationResult = new ValidationResult();
        var store = new ValidationConstructionStore();
        var executionStore = new ValidationExecutionStore();
        var allItems = new List<ExpandedItem>();

        foreach (var mainValidator in mainValidators)
        {
            var expandedItems = mainValidator.Store.Compile(instance);
            if (expandedItems?.Any() == true)
            {
                allItems.AddRange(expandedItems);
            }
        }

        var groupedItems = allItems
            .GroupBy(x => new { x.PropertyName });

        var vitallyFailedFields = new List<string>();
        foreach (var validationObjectGroup in groupedItems)
        {
            foreach (var propertyGroup in validationObjectGroup)
            {
                if (vitallyFailedFields
                    .Any(f => propertyGroup.PropertyName.StartsWith(f))
                )
                {
                    continue;
                }
                
                if (await propertyGroup.CanValidate(instance)) 
                {
                    var result = propertyGroup.Validate(instance);

                    if (!result.Success)
                    {
                        messageProcessor.ProcessErrorMessage(result);

                        validationResult.AddItem(propertyGroup, result);

                        if (propertyGroup.IsVital)
                        {
                            vitallyFailedFields.Add(propertyGroup.PropertyName);
                            break;
                        }
                    }
                    var failedFields = result.GetFailedDependantFields(propertyGroup.PropertyName, validationResult);
                    if (failedFields?.Any() == true)
                    {
                        if (propertyGroup.IsVital)
                        {
                            vitallyFailedFields.AddRange(failedFields);
                            break;
                        }
                    }
                }
            }
        }

        return validationResult;
    }

    public DescriptionResult Describe() 
    {
        var descriptionResult = new DescriptionResult();
        var store = new ValidationConstructionStore();
        var executionStore = new ValidationExecutionStore();
        var allItems = new List<ExpandedItem>();

        foreach (var mainValidator in mainValidators)
        {
            var expandedItems = mainValidator.Store.Describe();//ExpandToDescribe();
            if (expandedItems?.Any() == true)
            {
                allItems.AddRange(expandedItems);
            }
        }

        var groupedItems = allItems
            .GroupBy(x => new { x.PropertyName });

        foreach (var validationObjectGroup in groupedItems)
        {
            foreach (var propertyGroup in validationObjectGroup)
            {
                var result = propertyGroup.Describe();
                messageProcessor.ProcessDescriptionMessage(result);
                descriptionResult.AddItem(propertyGroup, result);
            }
        }

        return descriptionResult;
    }
}


public class NullLanguageSource : ILanguageSource
{
    public List<string> GetCultures()
    {
        return new List<string>();
    }

    public string GetDescriptionTranslation(string key, string originalMessage)
    {
        return originalMessage;
    }

    public string GetErrorTranslation(string key, string originalMessage)
    {
        return originalMessage;
    }
}

public class LangaugeConfiguration
{
    private readonly List<ILanguageSource> languageSources = new();
    public string? CurrentCulture { get; protected set; } = null;
    public ILanguageSource CurrentLanguage { get; protected set; } = new NullLanguageSource();

    public void SetCurrentCulture(string? culture) 
    {
        if (culture == null)
        {
            CurrentCulture = culture;
            CurrentLanguage = new NullLanguageSource();
        }
        else
        {
            var foundLanguage = languageSources
                .FirstOrDefault(x =>
                    x.GetCultures().Any(c => c.StartsWith(culture))
                );

            if (foundLanguage != null)
            {
                CurrentCulture = culture;
                CurrentLanguage = foundLanguage;
            }
            else
            {
                throw new Exception("Current Culture not found.");
            }
        }
    }

    public void AddLanguages(params ILanguageSource[] languages)
    {
        languageSources.InsertRange(0, languages);
    }   
}

public class Configuration
{
    public LangaugeConfiguration Language { get; set; } = new();
}

public sealed class PopValidations
{
    public static Configuration Configuation { get; } = new Configuration();
}

public interface ILanguageSource
{
    List<string> GetCultures();
    string GetErrorTranslation(string key, string originalMessage);
    string GetDescriptionTranslation(string key, string originalMessage);
}

public class MessageProcessor
{
    public MessageProcessor(){}

    public string ProcessErrorMessage(
        string validatorName,
        string message, 
        List<KeyValuePair<string, string>> keyValues
    )
    {
        message = PopValidations.Configuation.Language.CurrentLanguage
            .GetErrorTranslation(validatorName, message);

        foreach (var keyValue in keyValues)
        {
            message = message.Replace(keyValue.Key, keyValue.Value);
        }

        return message;
    }

    public string ProcessDescription(
        string validatorName,
        string message,
        List<KeyValuePair<string, string>> keyValues
    )
    {
        message = PopValidations.Configuation.Language.CurrentLanguage
            .GetDescriptionTranslation(validatorName, message);

        foreach (var keyValue in keyValues)
        {
            message = message.Replace(keyValue.Key, keyValue.Value);
        }

        return message;
    }

    internal void ProcessErrorMessage(ValidationActionResult result)
    {
        result.UpdateMessageProcessor(
            ProcessErrorMessage(
                result.Validator,
                result.Message,
                result.KeyValues 
            )
        );
    }

    internal void ProcessDescriptionMessage(DescribeActionResult result)
    {
        result.UpdateDescription(
            ProcessDescription(
                result.Validator,
                result.Message,
                result.KeyValues
            )
        );
    }
}


public class BasicLevelTests
{
    [Fact]
    public void BasicValidator_Describe_ToJson()
    {
        // Arrange
        var validator = new AlbumValidator();
        var runner = new ValidationRunner<Album>(
            new List<IMainValidator<Album>>() { validator },
            new MessageProcessor()
        );

        // Act
        var results = runner.Describe();
        var json = new JsonConverter().ToJson(results);

        // Assert
        Approvals.VerifyJson(json);
    }

    [Fact]
    public async Task BasicValidator_Validate_ToJson()
    {
        // Arrange
        var validator = new AlbumValidator();
        var runner = new ValidationRunner<Album>(
            new List<IMainValidator<Album>>() { validator },
            new MessageProcessor()
        );

        // Act
        var results = await runner.Validate(new Album());
        var json = new JsonConverter().ToJson(results);

        // Assert
        Approvals.VerifyJson(json);
    }
}