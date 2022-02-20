using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Scopes;
using Validations.Utilities;
using Validations.Validations;

namespace Validations.Internal;

public interface IFieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType>
{
    //void AddValidator(IValidationWrapper<TValidationType> validation);
    //List<IValidationWrapper<TValidationType>> GetValidations();
    //object? GetPropertyValue(object instance);
    //string Property { get; }
    //bool Matches<TResult>(Expression<Func<TValidationType, TResult>> action);
}

public interface IFieldDescriptor<TValidationType>
{
    void AddValidator(IValidationWrapper<TValidationType> validation);
    List<IValidationWrapper<TValidationType>> GetValidations();
    object? GetPropertyValue(object instance);
    string Property { get; }
    bool Matches<TResult>(Expression<Func<TValidationType, TResult>> action);
    IScope<TValidationType> GetCurrentScope();
    IFieldDescriptorScope<TValidationType> GetFieldDescriptorScope();
}

internal class FieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType>
{
    private List<IValidationWrapper<TValidationType>> Validations { get; } = new();
    private Expression<Func<TValidationType, TFieldType>> propertyAcessor;
    public string Property { get; }
    private readonly IValidationScope<TValidationType> validationScope;

    public FieldDescriptor(IValidationScope<TValidationType> validationScope, Expression<Func<TValidationType, TFieldType>> property)
    {
        if (property == null || property.Body == null)
            throw new ArgumentNullException(nameof(property));

        Property = ExpressionUtilities.GetPropertyPath(property);
        this.validationScope = validationScope;
        propertyAcessor = property;
    }

    public List<IValidationWrapper<TValidationType>> GetValidations() { return Validations; }

    public bool Matches<TResult2>(Expression<Func<TValidationType, TResult2>> action)
    {
        return Property == ExpressionUtilities.GetPropertyPath(action);
    }

    public object? GetPropertyValue(object instance)
    {
        if (instance is TValidationType convert)
        {
            return propertyAcessor.Compile().Invoke(convert);
        }
        return null;
    }

    public void AddValidator(IValidationWrapper<TValidationType> validation)
    {
        Validations.Add(validation);
    }

    public IScope<TValidationType> GetCurrentScope()
    {
        return validationScope.GetExecutingScope();
    }
    
    IFieldDescriptorScope<TValidationType> IFieldDescriptor<TValidationType>.GetFieldDescriptorScope()
    {
        return validationScope as IFieldDescriptorScope<TValidationType>;
    }
}