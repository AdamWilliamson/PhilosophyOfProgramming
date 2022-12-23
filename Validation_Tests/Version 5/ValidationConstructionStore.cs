using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Validations_Tests.Version_5;

public interface IExpandableEntity 
{
    Func<IValidatableStoreItem, IValidatableStoreItem>? Decorator { get; }
    //IParentScope Parent { get; }
    void ExpandToValidate(ValidationConstructionStore store, object? value);
    void ExpandToDescribe(ValidationConstructionStore store);
    void AsVital();
    bool IgnoreScope { get; }
}

//public interface IConstructionStoreListItem : IExpandableEntity
//{
//    bool IsExpandable { get; }
//    bool IsValidationComponent { get; }
//}

public interface IStoreItem 
{
    ScopeParent ScopeParent { get; }
    IFieldDescriptorOutline? FieldDescriptor { get; }
}
public interface IExpandableStoreItem : IStoreItem, IExpandableEntity
{
    IExpandableEntity Component { get; }
}

public class ExpandableStoreItem : IExpandableStoreItem
{
    public bool IgnoreScope => false;

    public ExpandableStoreItem(
        ScopeParent scopeParent,
        IFieldDescriptorOutline? fieldDescriptor,
        IExpandableEntity component
    )
    {
        ScopeParent = scopeParent;
        FieldDescriptor = fieldDescriptor;
        Component = component;
    }

    public void AsVital()
    {
        Component.AsVital();
    }

    public ScopeParent ScopeParent { get; }
    public IExpandableEntity Component { get; }
    public IFieldDescriptorOutline? FieldDescriptor { get; }
    public Func<IValidatableStoreItem, IValidatableStoreItem>? Decorator => Component.Decorator;
    //public IParentScope Parent => Component.Parent;

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        var newValue = FieldDescriptor?.GetValue(value) ?? value;
        Component.ExpandToValidate(store, newValue);
    }

    public void ExpandToDescribe(ValidationConstructionStore store)
    {
        Component.ExpandToDescribe(store);
    }
}

public class ExpandedItem 
{
    private readonly IValidatableStoreItem validatableStoreItem;

    public ExpandedItem(IValidatableStoreItem validatableStoreItem)
    {
        this.validatableStoreItem = validatableStoreItem;
    }

    public ScopeParent? ScopeParent => validatableStoreItem.ScopeParent;

    public string PropertyName => validatableStoreItem.FieldDescriptor.PropertyName;

    public string FullAddressableName 
    {
        get
        {
            return validatableStoreItem.CurrentFieldExecutor?.FieldDescriptor?.PropertyName
                ?? PropertyName;
        }
    }

    public Task<bool> CanValidate(object? instance)
    {
        return validatableStoreItem.CanValidate(instance);
    }

    public ValidationActionResult Validate(object? instance)
    {
        var data = validatableStoreItem.GetValue(instance);
        return validatableStoreItem.Validate(data);
    }

    public DescribeActionResult Describe()
    {
        return validatableStoreItem.Describe();
    }

    public bool IsVital => validatableStoreItem.IsVital;

}

public interface IValidatableStoreItem : IStoreItem 
{
    bool IsVital { get; }
    ScopeParent? ScopeParent { get; set; }
    //IValidationComponent? GetValidationAction();
    IValidatableStoreItem? GetChild();
    FieldExecutor CurrentFieldExecutor { get; set; }
    void SetParent(FieldExecutor fieldExecutor);
    IFieldDescriptorOutline FieldDescriptor { get; set; }
    IValidationComponent Component { get; }
    object? GetValue(object? value);

    Task<bool> CanValidate(object? instance);
    ValidationActionResult Validate(object? value);
    Task InitScopes(object? instance);
    DescribeActionResult Describe();
    void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor);
}

public class ValidatableStoreItem : IValidatableStoreItem
{
    public ValidatableStoreItem(
        bool isVital,
        FieldExecutor currentFieldExecutor,
        IFieldDescriptorOutline fieldDescriptor,
        ScopeParent scopeParent,
        IValidationComponent component
    )
    {
        IsVital = isVital;
        CurrentFieldExecutor = currentFieldExecutor;
        FieldDescriptor = fieldDescriptor;
        ScopeParent = scopeParent;
        Component = component;
    }

    public ValidatableStoreItem(
        bool isVital,
        FieldExecutor currentFieldExecutor,
        IFieldDescriptorOutline fieldDescriptor,
        ScopeParent scopeParent,
        IValidatableStoreItem childStoreItem
)
    {
        IsVital = isVital;
        CurrentFieldExecutor = currentFieldExecutor;
        FieldDescriptor = fieldDescriptor;
        ScopeParent = scopeParent;
        ChildStoreItem = childStoreItem;
        Component = childStoreItem.Component;
    }

    public bool IsVital { get; }
    public FieldExecutor CurrentFieldExecutor { get; set; }
    public IFieldDescriptorOutline FieldDescriptor { get; set; }
    public ScopeParent? ScopeParent { get; set; }
    public void SetParent(FieldExecutor fieldExecutor)
    {
        if(CurrentFieldExecutor == null)
        {
            CurrentFieldExecutor = new FieldExecutor(fieldExecutor, FieldDescriptor);
        }
        else
        {
            CurrentFieldExecutor.SetParent(fieldExecutor);
        }
    }
    public IValidatableStoreItem? ChildStoreItem { get; }
    public IValidationComponent? Component { get; }

    public IValidatableStoreItem? GetChild()
    {
        return ChildStoreItem;
    }

    public object? GetValue(object? value)
    {
        if (CurrentFieldExecutor != null)
        {
            return FieldDescriptor.GetValue(CurrentFieldExecutor.GetValue(value));
        }
        return FieldDescriptor.GetValue(value);
    }

    public void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor)
    {
        if (ChildStoreItem != null)
        {
            ChildStoreItem.ReHomeScopes(attemptedScopeFieldDescriptor);
        }

        Component.ReHomeScopes(attemptedScopeFieldDescriptor);
    }
    public Task InitScopes(object? instance)
    {

        if (ChildStoreItem != null)
        {
            return ChildStoreItem.InitScopes(instance);
        }

        return Component.InitScopes(instance);
    }
    public Task<bool> CanValidate(object? instance) 
    {
        
        return Task.FromResult(true); 
    }

    public virtual ValidationActionResult Validate(object? value)
    {
        if (ChildStoreItem != null)
        {
            return ChildStoreItem.Validate(value);
        }

        return Component.Validate(value);
    }
    public virtual DescribeActionResult Describe() {
        if (ChildStoreItem != null)
        {
            return ChildStoreItem.Describe();
        }

        return Component.Describe();
    }
}

public class FieldExecutor
{
    object? RetrievedValue = null;
    bool ValueHasBeenRetrieved = false;

    public FieldExecutor(
        FieldExecutor? parent,
        IFieldDescriptorOutline fieldDescriptor
    )
    {
        Parent = parent;
        FieldDescriptor = fieldDescriptor;
    }

    public void SetParent(FieldExecutor? newParent)
    {
        if (Parent == null)
        {
            Parent = newParent;
        }
        else
        {
            Parent.SetParent(newParent);
        }
    }

    public FieldExecutor? Parent { get; protected set; }
    public IFieldDescriptorOutline FieldDescriptor { get; }

    public object? GetValue(object? value)
    {
        if (ValueHasBeenRetrieved) return RetrievedValue;

        object? result;

        if (Parent != null)
        {
            result = FieldDescriptor.GetValue(Parent.GetValue(value));
            
        }
        else
            result = FieldDescriptor.GetValue(value);

        RetrievedValue = result;
        ValueHasBeenRetrieved = true;
        return result;
    }
}

public class BaseParent : IParentScope
{
    public Guid Id => Guid.Empty;

    public IParentScope? Parent => null;

    public string Name => "";
}

public class ScopeParent
{
    public ScopeParent? PreviousScope { get; }
    public IParentScope? CurrentScope { get; }
    
    public ScopeParent(IParentScope? currentScope, ScopeParent? PreviousScope)
    {
        CurrentScope = currentScope;
        this.PreviousScope = PreviousScope;
    }
}

public class ScopeChangeDecorator : IValidatableStoreItem
{
    private readonly IValidatableStoreItem previous;

    public ScopeChangeDecorator(
        IFieldDescriptorOutline scopeFieldDescriptor,
        IValidatableStoreItem previous)
    {
        ScopeFieldDescriptor = scopeFieldDescriptor;
        this.previous = previous;
    }

    public bool IsVital => previous.IsVital;

    public ScopeParent? ScopeParent { 
        get => previous.ScopeParent; 
        set => previous.ScopeParent = value; 
    }

    public FieldExecutor CurrentFieldExecutor
    {
        get { return previous.CurrentFieldExecutor; }
        set { previous.CurrentFieldExecutor = value; }
    }

    public IFieldDescriptorOutline FieldDescriptor
    {
        get { return previous.FieldDescriptor; }
        set { previous.FieldDescriptor = value; }
    }

    public IValidationComponent Component => previous.Component;

    public IFieldDescriptorOutline ScopeFieldDescriptor { get; }

    ScopeParent IStoreItem.ScopeParent => previous.ScopeParent;

    public Task<bool> CanValidate(object? instance)
    {
        var value = ScopeFieldDescriptor?.GetValue(instance) ?? instance;
        return previous.CanValidate(value);
    }

    public DescribeActionResult Describe()
    {
        return previous.Describe();
    }

    public IValidatableStoreItem? GetChild()
    {
        return previous.GetChild();
    }

    public object? GetValue(object? value)
    {
        return previous.GetValue(value);
    }

    public Task InitScopes(object? instance)
    {
        return previous.InitScopes(instance);
    }

    public void ReHomeScopes(IFieldDescriptorOutline attemptedScopeFieldDescriptor)
    {
        previous.ReHomeScopes(attemptedScopeFieldDescriptor);
    }

    public void SetParent(FieldExecutor fieldExecutor)
    {
        previous.SetParent(fieldExecutor);
    }

    public ValidationActionResult Validate(object? value)
    {
        return previous.Validate(value);
    }
}

public interface IValidationCompilationStore 
{
    List<ExpandedItem> Compile<TValidationType>(TValidationType? instance);
    List<ExpandedItem> Describe();
}

// Used during construction of validators
public class ValidationConstructionStore : IValidationCompilationStore
{
    public List<IStoreItem> unExpandedItems = new();
    private Stack<Func<IValidatableStoreItem, IValidatableStoreItem>?> Decorators = new();
    private Stack<ScopeParent> ScopeParents = new();
    private Stack<IFieldDescriptorOutline> FieldParents = new();
    private Stack<FieldExecutor> FieldExecutors = new();
    
    public FieldExecutor? GetCurrentFieldExecutor(IFieldDescriptorOutline ignore = null)
    {
        if (FieldExecutors.Any())
            return FieldExecutors
                .Where(x => x is not null)
                .Where(x => x.FieldDescriptor != ignore)
                .FirstOrDefault();
        return null;
    }

    public void PushFieldDescriptor(IFieldDescriptorOutline fieldDescriptor)
    {
        FieldParents.Push(fieldDescriptor);
        FieldExecutors.Push(
            new FieldExecutor(
                GetCurrentFieldExecutor(fieldDescriptor), 
                fieldDescriptor
            )
        );

        Decorators.Push((previous) => new ScopeChangeDecorator(fieldDescriptor, previous));
    }

    public void PopFieldDescriptor()
    {
        FieldParents.Pop();
        FieldExecutors.Pop();
        Decorators.Pop();
    }

    public ScopeParent? GetCurrentScopeParent()
    {
        if (!ScopeParents.Any()) return null;
        return ScopeParents.Peek();
    }

    public void PushParent(IParentScope? parent)
    {
        ScopeParent? previous = null;
        if (ScopeParents.Any())
            previous = ScopeParents.Peek();

        //if (parent == null) return;
        ScopeParents.Push(new ScopeParent(parent, previous));
    }

    public void PopParent()
    {
        ScopeParents.Pop();
    }

    public void PushDecorator(Func<IValidatableStoreItem, IValidatableStoreItem>? decorator)
    {
        Decorators.Push(decorator);
    }

    public void PopDecorator()
    {
        Decorators.Pop();
    }

    public void AddItem(IFieldDescriptorOutline? fieldDescriptor, IExpandableEntity component)
    {
        var scopeParent = new ScopeParent(component as IParentScope, GetCurrentScopeParent());
        IExpandableStoreItem decoratedItem = new ExpandableStoreItem(scopeParent, fieldDescriptor, component);
        unExpandedItems.Add(decoratedItem);
    }

    private int PushParentTree(ScopeParent scopeParent) 
    {
        int value = 1;
        if (scopeParent?.PreviousScope != null)
        {
            value += PushParentTree(scopeParent.PreviousScope);
        }
        PushParent(scopeParent?.CurrentScope);
        return value;
    }

    private void PopCount(int x)
    {
        for(int i = 0; i < x; i++)
        {
            ScopeParents.Pop();
        }
    }

    public void AddItemToCurrentScope(IStoreItem storeItem)
    {
        //IStoreItem? decoratedItem = null;
        if (storeItem is IExpandableStoreItem expandable && expandable is not null)
        {
            var currentParent = GetCurrentScopeParent();
            //expandable.ReHomeScopedData(currentParent);
            AddItem(GenerateName(expandable.FieldDescriptor), expandable.Component);

            //var scopeParent = new ScopeParent(
            //    expandable.Component as IParentScope, 
            //    GetCurrentScopeParent()
            //);
            //IExpandableStoreItem decoratedItem = new ExpandableStoreItem(
            //    scopeParent,
            //    GenerateName(expandable.FieldDescriptor)
            //    , 
            //    expandable);
            //unExpandedItems.Add(decoratedItem);
        }
        else if (storeItem is IValidatableStoreItem validatable)
        {
            //PushParent(validatable.ScopeParent?.CurrentScope);
            int pushedParents = PushParentTree(validatable.ScopeParent);
            validatable.SetParent(GetCurrentFieldExecutor());
            FieldExecutors.Push(validatable.CurrentFieldExecutor);

            //AddItem(validatable.IsVital,
            //    GenerateName(validatable.FieldDescriptor),
            //    validatable);
            validatable.FieldDescriptor = GenerateName(validatable.FieldDescriptor);
            var scopeParent = new ScopeParent(
                validatable.ScopeParent?.CurrentScope,//.Component as IParentScope, 
                GetCurrentScopeParent()
            );
            validatable.ScopeParent = scopeParent;
            validatable.CurrentFieldExecutor = GetCurrentFieldExecutor();
            //IExpandableStoreItem decoratedItem = new ExpandableStoreItem(
            //    scopeParent, 
            //    validatable.FieldDescriptor,
            //    validatable);
            IValidatableStoreItem decoratedItem = validatable;
            foreach (var decorator in Decorators.Where(d => d is not null))
            {
                // if (decoratedItem != null)// && decoratedItem is IValidatableStoreItem converted)
                {
                    //IValidatableStoreItem newItem = new ValidatableStoreItem(isVital, fieldDescriptor, currentParent, converted);
                    //if (newItem != null)
                    //{
                    decoratedItem = decorator?.Invoke(decoratedItem) ?? decoratedItem;
                    //}
                }
            }

            unExpandedItems.Add(decoratedItem);

            //PopParent();
            FieldExecutors.Pop();
            PopCount(pushedParents);
            //decoratedItem = new ValidatableStoreItem(
            //    validatable.IsVital,
            //    validatable.FieldDescriptor,
            //    GetCurrentScopeParent(),
            //    validatable.Component);

        }

        //if (decoratedItem != null)
        //    unExpandedItems.Add(decoratedItem);
    }

    public void AddItem(IStoreItem storeItem)
    {
        unExpandedItems.Add(storeItem);
    }

    //public void AddItem(bool isVital, IFieldDescriptorOutline fieldDescriptor, IParentScope currentParent, IValidationComponent component)
    //{
    //    IValidatableStoreItem decoratedItem = new ValidatableStoreItem(isVital, fieldDescriptor, currentParent, component);
    //    foreach (var decorator in Decorators.Where( d => d is not null))
    //    {
    //        if (decoratedItem != null && decoratedItem is IValidationComponent converted)
    //        {
    //            IValidatableStoreItem newItem = new ValidatableStoreItem(isVital, fieldDescriptor, currentParent, converted);
    //            if (newItem != null)
    //            {
    //                decoratedItem = decorator?.Invoke(newItem) ?? newItem;
    //            }
    //        }
    //    }

    //    unExpandedItems.Add(decoratedItem);
    //}

    public void AddItem(
        bool isVital, 
        IFieldDescriptorOutline fieldDescriptor,
        IValidationComponent component)
    {
        IValidatableStoreItem decoratedItem = new ValidatableStoreItem(
            isVital,
            GetCurrentFieldExecutor(),
            fieldDescriptor, 
            GetCurrentScopeParent(), 
            component);

        //IValidatableStoreItem decoratedItem = new ValidatableStoreItem(
        //    isVital, fieldDescriptor, currentParent, component);
        foreach (var decorator in Decorators.Where(d => d is not null))
        {
           // if (decoratedItem != null)// && decoratedItem is IValidatableStoreItem converted)
            {
                //IValidatableStoreItem newItem = new ValidatableStoreItem(isVital, fieldDescriptor, currentParent, converted);
                //if (newItem != null)
                //{
                    decoratedItem = decorator?.Invoke(decoratedItem) ?? decoratedItem;
                //}
            }
        }

        AddItem( decoratedItem);
        //unExpandedItems.Add(decoratedItem);
    }

    public List<IStoreItem> GetItems() { return unExpandedItems; }

    public void Merge(ValidationConstructionStore store)
    {
        unExpandedItems.AddRange(store.GetItems());
    }

    internal List<IValidatableStoreItem> ExpandToValidate<TValidationType>(TValidationType? instance)
    {
        PushParent(null);
        var originalItems = unExpandedItems.ToList();
        unExpandedItems = new();
        var results = new List<IValidatableStoreItem>();
        
        foreach(var item in originalItems)
        {
            //item.ExpandToValidate(this);
            var funcResults = ExpandToValidateRecursive(item, instance);
            if(funcResults?.Any() == true)
            {
                results.AddRange(funcResults);
            }
        }

        return results;
    }

    public List<ExpandedItem> Compile<TValidationType>(TValidationType? instance)
    {
        return ExpandToValidate(instance)
            .Select(r => new ExpandedItem(r))
            .ToList();
    }

    public List<ExpandedItem> Describe()
    {
        return ExpandToDescribe()
            .Select(r => new ExpandedItem(r))
            .ToList();
    }


    internal List<IValidatableStoreItem> ExpandToDescribe()
    {
        PushParent(null);
        var originalItems = unExpandedItems.ToList();
        unExpandedItems = new();
        var results = new List<IValidatableStoreItem>();

        foreach (var item in originalItems)
        {
            //item.ExpandToValidate(this);
            var funcResults = ExpandToDescribeRecursive(item);
            if (funcResults?.Any() == true)
            {
                results.AddRange(funcResults);
            }
        }

        return results;
    }

    internal List<IValidatableStoreItem> ExpandToDescribeRecursive(
        IStoreItem storeItem
        )
    {
        var results = new List<IValidatableStoreItem>();

        if (storeItem is IValidatableStoreItem converted)
        {
            var attemptedScopeFieldDescriptor = GetCurrentFieldExecutor()?.FieldDescriptor;
            converted.ReHomeScopes(attemptedScopeFieldDescriptor);
            return new() { converted };
        }
        else if (storeItem is IExpandableStoreItem expandable)
        {
            if (!expandable.Component.IgnoreScope)
            {
                PushDecorator(expandable.Decorator);
                PushParent(expandable.ScopeParent?.CurrentScope);

                if (storeItem.FieldDescriptor != null)
                    PushFieldDescriptor(storeItem.FieldDescriptor);
            }

            expandable.ExpandToDescribe(this);
            var copyNewUnExpanded = unExpandedItems.ToList();
            unExpandedItems = new();

            foreach (var item in copyNewUnExpanded)
            {
                var recursiveResponse = ExpandToDescribeRecursive(item);
                if (recursiveResponse?.Any() == true)
                {
                    results.AddRange(recursiveResponse);
                }
            }

            if (!expandable.Component.IgnoreScope)
            {
                if (storeItem.FieldDescriptor != null)
                    PopFieldDescriptor();
                PopParent();
                PopDecorator();
            }
        }

        return results;
    }
    //{
    //    var results = new List<IValidatableStoreItem>();

    //    if (storeItem is IValidatableStoreItem converted) { return new() { converted }; }
    //    else if (storeItem is IExpandableStoreItem expandable)
    //    {
    //        PushDecorator(expandable.Decorator);
    //        PushParent(expandable.ScopeParent?.CurrentScope);
    //        if (storeItem.FieldDescriptor != null)
    //            PushFieldDescriptor(storeItem.FieldDescriptor);
    //        //FieldParents.Push(storeItem.FieldDescriptor);

    //        expandable.ExpandToDescribe(this);
    //        var copyNewUnExpanded = unExpandedItems.ToList();
    //        unExpandedItems = new();

    //        foreach (var item in copyNewUnExpanded)
    //        {
    //            var recursiveResponse = ExpandToDescribeRecursive(item);
    //            if (recursiveResponse?.Any() == true)
    //            {
    //                results.AddRange(recursiveResponse
    //                    .Select(x => new ValidatableStoreItem(
    //                        x.IsVital,
    //                        GetCurrentFieldExecutor(),
    //                        x.FieldDescriptor,
    //                        x.ScopeParent,
    //                        x.Component))
    //                    );
    //            }
    //        }

    //        //FieldParents.Pop();
    //        if (storeItem.FieldDescriptor != null)
    //            PopFieldDescriptor();
    //        PopParent();
    //        PopDecorator();
    //    }

    //    return results;
    //}

    private ValidationFieldDescriptorOutline GenerateName(IFieldDescriptorOutline? outline)
    {
        if (outline == null) return null;
        //string name = string.Join(".", FieldParents.Where(x => x is not null));
        if (!FieldParents.Where(x => x is not null).Any())
            return new ValidationFieldDescriptorOutline(outline.PropertyName, outline);

        var ownerField = FieldParents.Where(x => x is not null).First();
        return new ValidationFieldDescriptorOutline(outline.AddTo(ownerField.PropertyName), outline);
    }

    internal List<IValidatableStoreItem> ExpandToValidateRecursive<TValidationType>(
        IStoreItem storeItem,
        TValidationType? instance
        )
    {
        var results = new List<IValidatableStoreItem>();

        if (storeItem is IValidatableStoreItem converted) {
            var attemptedScopeFieldDescriptor = GetCurrentFieldExecutor()?.FieldDescriptor;
            converted.ReHomeScopes(attemptedScopeFieldDescriptor);
            return new() { converted }; 
        }
        else if (storeItem is IExpandableStoreItem expandable) 
        {
            if (!expandable.Component.IgnoreScope)
            {
                PushDecorator(expandable.Decorator);
                PushParent(expandable.ScopeParent?.CurrentScope);
                //FieldParents.Push(storeItem.FieldDescriptor);
                if (storeItem.FieldDescriptor != null)
                    PushFieldDescriptor(storeItem.FieldDescriptor);
            }
            //var newScopesValue = storeItem?.FieldDescriptor?.GetValue(instance) ?? instance;
            expandable.ExpandToValidate(this, instance);
            var copyNewUnExpanded = unExpandedItems.ToList();
            unExpandedItems = new();

            foreach(var item in copyNewUnExpanded)
            {
                var recursiveResponse = ExpandToValidateRecursive(item, instance);
                if (recursiveResponse?.Any() == true)
                {
                    results.AddRange(recursiveResponse);
                        //.Select(x => new ValidatableStoreItem(
                        //    x.IsVital,
                        //    x.CurrentFieldExecutor,
                        //    x.FieldDescriptor, 
                        //    x.ScopeParent, 
                        //    x.Component))
                        //);
                }
            }

            if (!expandable.Component.IgnoreScope)
            {
                if (storeItem.FieldDescriptor != null)
                    PopFieldDescriptor();
                //FieldParents.Pop();
                PopParent();
                PopDecorator();
            }
        }

        return results;
    }

    class ValidationFieldDescriptorOutline : IFieldDescriptorOutline
    {
        private readonly IFieldDescriptorOutline outline;

        public ValidationFieldDescriptorOutline(string propertyName, IFieldDescriptorOutline outline)
        {
            PropertyName = propertyName;
            this.outline = outline;
        }

        public string PropertyName { get; }

        public string AddTo(string existing)
        {
            throw new NotImplementedException();
        }

        public object? GetValue(object? input)
        {
            return outline.GetValue(input);
        }
    }
}
