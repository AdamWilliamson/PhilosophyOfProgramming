using System;
using System.Collections.Generic;
using System.Linq;

namespace Validations_Tests.Version_5;

public interface IExpandableEntity 
{
    Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? Decorator { get; }
    //IParentScope Parent { get; }
    void ExpandToValidate(ValidationConstructionStore store, object? value);
    void ExpandToDescribe(ValidationConstructionStore store);

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

    public ScopeParent ScopeParent { get; }
    public IExpandableEntity Component { get; }
    public IFieldDescriptorOutline? FieldDescriptor { get; }
    public Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? Decorator => Component.Decorator;
    //public IParentScope Parent => Component.Parent;

    public void ExpandToValidate(ValidationConstructionStore store, object? value)
    {
        Component.ExpandToValidate(store, value);
    }

    public void ExpandToDescribe(ValidationConstructionStore store)
    {
        Component.ExpandToDescribe(store);
    }
}
public interface IValidatableStoreItem : IStoreItem 
{
    bool IsVital { get; }
    ScopeParent? ScopeParent { get; }
    IValidationAction? GetValidationAction();
    IValidatableStoreItem? GetChild();
    IFieldDescriptorOutline FieldDescriptor { get; }
    IValidationComponent Component { get; }
}

public class ValidatableStoreItem : IValidatableStoreItem
{
    public ValidatableStoreItem(
        bool isVital,
        IFieldDescriptorOutline fieldDescriptor,
        ScopeParent scopeParent,
        IValidationComponent component
    )
    {
        IsVital = isVital;
        FieldDescriptor = fieldDescriptor;
        ScopeParent = scopeParent;
        Component = component;
    }

    public ValidatableStoreItem(
        bool isVital,
        IFieldDescriptorOutline fieldDescriptor,
        ScopeParent scopeParent,
        IValidatableStoreItem childStoreItem
)
    {
        IsVital = isVital;
        FieldDescriptor = fieldDescriptor;
        ScopeParent = scopeParent;
        ChildStoreItem = childStoreItem;
        Component = childStoreItem.Component;
    }

    public bool IsVital { get; }
    public IFieldDescriptorOutline FieldDescriptor { get; }
    public ScopeParent? ScopeParent { get; }
    public IValidatableStoreItem? ChildStoreItem { get; }
    public IValidationComponent? Component { get; }

    public IValidationAction? GetValidationAction()
    {
        if (ChildStoreItem != null)
        {
            return ChildStoreItem.GetValidationAction();
        }

        return Component?.GetValidationAction();
    }

    public IValidatableStoreItem? GetChild()
    {
        return ChildStoreItem;
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

// Used during construction of validators
public class ValidationConstructionStore
{
    public List<IStoreItem> unExpandedItems = new();
    private Stack<Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>?> Decorators = new();
    private Stack<ScopeParent> ScopeParents = new();
    private Stack<IFieldDescriptorOutline> FieldParents = new();

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

    public void PushDecorator(Func<IValidatableStoreItem, ScopeParent, IValidatableStoreItem>? decorator)
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
        IStoreItem? decoratedItem = null;
        if (storeItem is IExpandableStoreItem expandable && expandable is not null)
        {
            
            AddItem(GenerateName(expandable.FieldDescriptor), expandable.Component);
            //decoratedItem = new ExpandableStoreItem(GetCurrentScopeParent(), expandable.Component);
        }
        else if (storeItem is IValidatableStoreItem validatable)
        {
            //PushParent(validatable.ScopeParent?.CurrentScope);
            int pushedParents = PushParentTree(validatable.ScopeParent);
            AddItem(validatable.IsVital,
                GenerateName(validatable.FieldDescriptor),
                validatable.Component);
            //PopParent();
            PopCount(pushedParents);
            //decoratedItem = new ValidatableStoreItem(
            //    validatable.IsVital,
            //    validatable.FieldDescriptor,
            //    GetCurrentScopeParent(),
            //    validatable.Component);
        }

        if (decoratedItem != null)
            unExpandedItems.Add(decoratedItem);
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
            fieldDescriptor, 
            GetCurrentScopeParent(), 
            component);

        foreach (var decorator in Decorators.Where(d => d is not null))
        {
            if (decoratedItem != null)// && decoratedItem is IValidationComponent converted)
            {
                //IValidatableStoreItem newItem = new ValidatableStoreItem(isVital,fieldDescriptor, currentParent, converted);
                //if (newItem != null)
                {
                    var result=  decorator?.Invoke(decoratedItem, GetCurrentScopeParent());
                    if (result != null) decoratedItem = result;
                }
            }
        }
        //expandedItems.Add(decoratedItem);
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

        if (storeItem is IValidatableStoreItem converted) { return new() { converted }; }
        else if (storeItem is IExpandableStoreItem expandable)
        {
            PushDecorator(expandable.Decorator);
            PushParent(expandable.ScopeParent?.CurrentScope);
            FieldParents.Push(storeItem.FieldDescriptor);

            expandable.ExpandToDescribe(this);
            var copyNewUnExpanded = unExpandedItems.ToList();
            unExpandedItems = new();

            foreach (var item in copyNewUnExpanded)
            {
                var recursiveResponse = ExpandToDescribeRecursive(item);
                if (recursiveResponse?.Any() == true)
                {
                    results.AddRange(recursiveResponse
                        .Select(x => new ValidatableStoreItem(
                            x.IsVital,
                            x.FieldDescriptor,
                            x.ScopeParent,
                            x.Component))
                        );
                }
            }

            FieldParents.Pop();
            PopParent();
            PopDecorator();
        }

        return results;
    }

    private ValidationFieldDescriptorOutline GenerateName(IFieldDescriptorOutline? outline)
    {
        if (outline == null) return null;
        //string name = string.Join(".", FieldParents.Where(x => x is not null));
        if (!FieldParents.Where(x => x is not null).Any())
            return new ValidationFieldDescriptorOutline(outline.PropertyName);

        var ownerField = FieldParents.Where(x => x is not null).First();
        return new ValidationFieldDescriptorOutline(outline.AddTo(ownerField.PropertyName));
    }

    internal List<IValidatableStoreItem> ExpandToValidateRecursive<TValidationType>(
        IStoreItem storeItem,
        TValidationType? instance
        )
    {
        var results = new List<IValidatableStoreItem>();

        if (storeItem is IValidatableStoreItem converted) { return new() { converted }; }
        else if (storeItem is IExpandableStoreItem expandable) 
        {
            PushDecorator(expandable.Decorator);
            PushParent(expandable.ScopeParent?.CurrentScope);
            FieldParents.Push(storeItem.FieldDescriptor);

            expandable.ExpandToValidate(this, instance);
            var copyNewUnExpanded = unExpandedItems.ToList();
            unExpandedItems = new();

            foreach(var item in copyNewUnExpanded)
            {
                var recursiveResponse = ExpandToValidateRecursive(item, instance);
                if (recursiveResponse?.Any() == true)
                {
                    results.AddRange(recursiveResponse
                        .Select(x => new ValidatableStoreItem(
                            x.IsVital,
                            x.FieldDescriptor, 
                            x.ScopeParent, 
                            x.Component))
                        );
                }
            }

            FieldParents.Pop();
            PopParent();
            PopDecorator();
        }

        return results;
    }

    class ValidationFieldDescriptorOutline : IFieldDescriptorOutline
    {
        public ValidationFieldDescriptorOutline(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        public string AddTo(string existing)
        {
            throw new NotImplementedException();
        }
    }
}
