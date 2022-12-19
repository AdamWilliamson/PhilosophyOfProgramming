using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Scopes;
using Validations.Utilities;
using Validations.Validations;

namespace Validations.Internal;

public interface IFieldDescriptor
{
    object? GetPropertyValue(object instance);
    string Property { get; }
    //bool Matches<TResult>(Expression<Func<TValidationType, TResult>> action);
    bool Matches(string property);
}

public interface IFieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType>
{
    //void AddValidator(IValidationWrapper<TValidationType> validation);
    //List<IValidationWrapper<TValidationType>> GetValidations();
    //object? GetPropertyValue(object instance);
    //string Property { get; }
    //bool Matches<TResult>(Expression<Func<TValidationType, TResult>> action);
    IFieldDescriptor<TValidationType, TFieldType> Vitally { get; }
}

//public interface IForEachFieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType>, IForEachFieldDescriptor<TValidationType>
//{
////    IFieldDescriptor<TValidationType, TFieldType> Vitally { get; }
//}

public interface IFieldDescriptor<TValidationType> : IFieldDescriptor
{
    void AddValidator(IValidationWrapper<TValidationType> validation);
    List<IValidationWrapper<TValidationType>> GetValidations();
    //object? GetPropertyValue(object instance);
    //string Property { get; }
    ////bool Matches<TResult>(Expression<Func<TValidationType, TResult>> action);
    //bool Matches(string property);
    IScope<TValidationType> GetCurrentScope();
    IFieldDescriptorScope<TValidationType> GetFieldDescriptorScope();
}

public interface IForEachFieldDescriptor<TValidationType> : IFieldDescriptor<TValidationType> {}

internal class FieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType>
{
    private List<IValidationWrapper<TValidationType>> Validations { get; } = new();
    private Func<TValidationType, TFieldType> propertyAcessor;
    public string Property { get; }
    private readonly IValidationScope<TValidationType> validationScope;
    private bool MakeNextVital = false;

    public FieldDescriptor(IValidationScope<TValidationType> validationScope, string property)
    {
        Property = property;
        this.validationScope = validationScope;
        propertyAcessor = ExpressionUtilities.GetAccessor<TValidationType, TFieldType>(property);
    }

    public List<IValidationWrapper<TValidationType>> GetValidations() { return Validations; }

    public bool Matches(string property) 
    {
        return Property == property;
    }
    //public bool Matches<TResult2>(Expression<Func<TValidationType, TResult2>> action)
    //{
    //    return Property == ExpressionUtilities.GetPropertyPath(action);
    //}

    public object? GetPropertyValue(object instance)
    {
        if (instance is TValidationType convert)
        {
            return propertyAcessor.Invoke(convert);
        }
        return null;
    }

    public void AddValidator(IValidationWrapper<TValidationType> validation)
    {
        if (MakeNextVital)
        {
            validation.MakeVital();
            MakeNextVital = false;
        }
        Validations.Add(validation);
    }

    public IScope<TValidationType>? GetCurrentScope()
    {
        return validationScope.GetExecutingScope();
    }
    
    IFieldDescriptorScope<TValidationType> IFieldDescriptor<TValidationType>.GetFieldDescriptorScope()
    {
        return validationScope as IFieldDescriptorScope<TValidationType>;
    }

    public IFieldDescriptor<TValidationType, TFieldType> Vitally 
    { 
        get
        {
            MakeNextVital = true;
            return this;
        } 
    }
}

internal class ChildFieldDescriptor<TValidationType, TChildObjectType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType>
{
    private List<IValidationWrapper<TValidationType>> Validations { get; } = new();
    private Func<TValidationType, TFieldType> propertyAcessor;
    public string Property { get; }
    private readonly IValidationScope<TValidationType> validationScope;
    private bool MakeNextVital = false;

    public ChildFieldDescriptor(IValidationScope<TValidationType> validationScope, string property)
    {
        Property = property;
        this.validationScope = validationScope;
        propertyAcessor = ExpressionUtilities.GetAccessor<TValidationType, TFieldType>(property);
    }

    public List<IValidationWrapper<TValidationType>> GetValidations() { return Validations; }

    public bool Matches(string property)
    {
        return Property == property;
    }
    //public bool Matches<TResult2>(Expression<Func<TValidationType, TResult2>> action)
    //{
    //    return Property == ExpressionUtilities.GetPropertyPath(action);
    //}

    public object? GetPropertyValue(object instance)
    {
        if (instance is TValidationType convert)
        {
            return propertyAcessor.Invoke(convert);
        }
        return null;
    }

    public void AddValidator(IValidationWrapper<TValidationType> validation)
    {
        if (MakeNextVital)
        {
            validation.MakeVital();
            MakeNextVital = false;
        }
        Validations.Add(validation);
    }

    public IScope<TValidationType>? GetCurrentScope()
    {
        return validationScope.GetExecutingScope();
    }

    IFieldDescriptorScope<TValidationType> IFieldDescriptor<TValidationType>.GetFieldDescriptorScope()
    {
        return validationScope as IFieldDescriptorScope<TValidationType>;
    }

    public IFieldDescriptor<TValidationType, TFieldType> Vitally
    {
        get
        {
            MakeNextVital = true;
            return this;
        }
    }
}