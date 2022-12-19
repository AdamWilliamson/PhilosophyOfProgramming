using System;
using System.Collections.Generic;
using System.Linq;

namespace Validations_Tests.Version_5;


public interface IValidationScopeExecutionStoreItem
{
    IParentScope ParentScope { get; }
    //string OwningType { get; }
    string PropertyName { get; }
    bool IsVital { get; }

    ValidationOutcome Validate();
}

public sealed class ValidatableEntityExecutionStoreItem : IValidationScopeExecutionStoreItem
{
    private IValidationAction ValidationAction { get; }
    public IParentScope ParentScope { get; }
    private IFieldDescriptorOutline FieldDescriptorOutline { get; }

    public ValidatableEntityExecutionStoreItem(
        IFieldDescriptorOutline fieldDescriptorOutline,
        bool isVital,
        IParentScope scope,
        IValidationAction validationComponent
    )
    {
        FieldDescriptorOutline = fieldDescriptorOutline;
        IsVital = isVital;
        ParentScope = scope;
        ValidationAction = validationComponent;
    }

    public bool IsVital { get; set; }

    //public string OwningType => FieldDescriptorOutline.PropertyOwner.Name;

    public string PropertyName => FieldDescriptorOutline.PropertyName;

    public ValidationOutcome Validate() => new ValidationOutcome(PropertyName, false, "Your a dick");
}

public class ValidationExecutionStore
{
    public List<IValidationScopeExecutionStoreItem> expandedItems = new();

    private Stack<Func<IValidationScopeExecutionStoreItem, IValidationScopeExecutionStoreItem>> Decorators = new();
    private Stack<IParentScope> Parents = new();

    public IParentScope GetCurrenParent() 
    { 
        return Parents.Peek(); 
    }
    public void PushParent(IParentScope parent)
    {
        Parents.Push(parent);
    }
    public void PopParent()
    {
        Parents.Pop();
    }

    public void PushDecorator(Func<IValidationScopeExecutionStoreItem, IValidationScopeExecutionStoreItem> decorator)
    {
        Decorators.Push(decorator);
    }

    public void PopDecorator()
    {
        Decorators.Pop();
    }

    public void AddItem(
        IFieldDescriptorOutline fieldDescriptorOutline,
        bool isVital,
        IParentScope scope,
        IValidationAction validationComponent)
    {
        expandedItems.Add(new ValidatableEntityExecutionStoreItem(
            fieldDescriptorOutline,
            isVital,
            scope,
            validationComponent
        ));
    }

    //public void AddItem(ValidationConstructionStoreItem constructionItems) 
    //{
    //    constructionItems.ExpandToValidate(this);
    //}

    //public void AddItem(IValidationScopeExecutionStoreItem item)
    //{
    //    var decoratedItem = item;
    //    foreach(var decorator in Decorators)
    //    {
    //        decoratedItem = decorator(decoratedItem);
    //    }

    //    expandedItems.Add(decoratedItem);
    //}
    
    public List<IValidationScopeExecutionStoreItem> GetItems() { return expandedItems; }
}
