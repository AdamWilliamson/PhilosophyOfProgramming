using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Internal;
using Validations.Utilities;
using Validations.Validations;

namespace Validations.Scopes
{
    public interface IValidationObject 
    {
        void Activate(IValidationContext context, object instance);
        ValidationMessage ActivateToDescribe(IValidationContext context);

        void Expand(IValidationContext context, object instance);
        void ExpandToDescribe(IValidationContext context);
    }

    public interface IParentScope
    {
        ScopeMessage GetValidationDetails();
    }

    public interface IScope<TValidationType> : IValidationObject, IParentScope
    {
        //void Activate(IValidationContext context, TValidationType instance);
        //void ActivateToDescribe(IValidationContext context);
        //List<IChildScope<TValidationType>> GetChildScopes();
        //ScopeMessage GetValidationDetails();
    }

    //public interface IFieldScope 
    //{
    //    string Property { get; }
    //}

    //public class FieldScope : IFieldScope
    //{
    //    public FieldScope(string property)
    //    {
    //        Property = property;
    //    }

    //    public string Property { get; }
    //}

    public interface IChildScope<TValidationType> : IScope<TValidationType>, IValidationObject
    {
        //void AddChildScope(IChildScope<TValidationType> scope);
        IParentScope GetParentScope();
        //List<IChildScope<TValidationType>> GetChildScopes();
        //void Activate(IValidationContext context, TValidationType instance);
        //void Describe(IValidationContext context);
        string? Description { get; }
        
    }

    public interface IFieldDescriptorScope<TValidationType>
    {
        //IFieldDescriptor<TValidationType, TFieldType> CreateFieldChainValidator<TFieldType>(Expression<Func<TValidationType, TFieldType>> property);
        //AbstractValidator<TFieldType> CreateFieldObjectValidator<TFieldType>(Expression<Func<TValidationType, TFieldType>> property);

        IFieldDescriptor<TValidationType, TFieldType> GetFieldDescriptor<TFieldType>(string propertyString);
        void AddFieldDescriptor(IFieldDescriptor<TValidationType> fieldDescriptor);

    }

    public interface IValidationScope<TValidationType> : IFieldDescriptorScope<TValidationType>
    {
        //void AddValidator(IValidationWrapper<TValidationType> validation);
        //void AddChildScope(IChildScope<TValidationType> scope);
        //void AddFieldScope(IFieldScope fieldScope);
        //List<IFieldScope> GetFieldScopes();
        void AddValidatableObject(IValidationObject validationObject);
        //string? Description { get; }
        void SetExecutingScope(IScope<TValidationType> scope);
        IScope<TValidationType>? GetExecutingScope();
        //LinkedList<IValidationObject> GetValidationObjects();

        void Expand(IValidationContext context, object instance);
        void ExpandToDescribe(IValidationContext context);
    }

    //public interface IExpansionScope { }

    //public class FieldValidationItem 
    //{
    //    public IFieldDescriptor FieldDescriptor { get; set; }
    //    public IValidationWrapper ValidationWrapper { get; set; }
    //}

    /// <summary>
    /// The Base scope for each individual Validator Class
    /// It contains functions for assigning validators to fields
    /// Manages child scopes
    /// Should not be supplied to Validator internals.
    /// This is for passing Validator information to the Runner.
    /// </summary>
    /// <typeparam name="TValidationType"></typeparam>
    public class ValidationScope<TValidationType> : IValidationScope<TValidationType>, IParentScope
    {
        protected LinkedList<IValidationObject> validationObjects = new();
        //protected List<IChildScope<TValidationType>> ChildScopes = new();
        //protected List<IFieldScope> FieldScopes = new();
        protected readonly List<IFieldDescriptor<TValidationType>> fieldChainValidators = new();
        //protected readonly LinkedList<FieldValidationItem> ValidationItems = new ();
        protected IScope<TValidationType>? currentScope = null;

        public ValidationScope(){}

        public LinkedList<IValidationObject> GetValidationObjects() { return validationObjects; }
        public virtual IScope<TValidationType>? GetParentScope() { return null; }

        public virtual ScopeMessage GetValidationDetails()
        {
            return new ScopeMessage(nameof(ValidationScope<TValidationType>), null, GetParentScope()?.GetValidationDetails(), new());
        }

        public virtual void SetExecutingScope(IScope<TValidationType> scope)
        {
            currentScope = scope;
        }

        public virtual IScope<TValidationType>? GetExecutingScope()
        {
            return currentScope;
        }

        //public virtual void Expand()
        //{
        //    //foreach()
        //}

        //public void SetExecutingScope(IValidationScope<TValidationType> scope)
        //{
        //    currentScope = scope;
        //}

        //public IValidationScope<TValidationType> GetExecutingScope()
        //{
        //    return currentScope;
        //}

        //public virtual void AddExpansionScope(IExpansionScope expansionScope) {  ExpansionScopes.AddLast(expansionScope); }

        //public virtual void AddFieldScope(IFieldScope fieldScope) { FieldScopes.Add(fieldScope); }

        //public virtual void AddChildScope(IChildScope<TValidationType> scope)
        //{
        //    //if (currentScope == null) ChildScopes.Add(scope);
        //    //else currentScope.AddScope(scope);

        //    ChildScopes.Add(scope);
        //}


        public void AddValidatableObject(IValidationObject validationObject)
        {
            validationObjects.AddLast(validationObject);
        }

        public void Expand(IValidationContext context, object instance)
        {
            foreach(var validationObject in validationObjects)
            {
                validationObject.Expand(context, instance);
            }

            context.AddValidationObjects(validationObjects);
        }

        public void ExpandToDescribe(IValidationContext context)
        {
            foreach (var validationObject in validationObjects)
            {
                validationObject.ExpandToDescribe(context);
            }

            context.AddValidationObjects(validationObjects);
        }

        public virtual IFieldDescriptor<TValidationType, TFieldType> GetFieldDescriptor<TFieldType>(Expression<Func<TValidationType, TFieldType>> property)
        {
            string propertyString = ExpressionUtilities.GetPropertyPath(property);
            return GetFieldDescriptor<TFieldType>(propertyString);
        }

        public virtual IFieldDescriptor<TValidationType, TFieldType> GetFieldDescriptor<TFieldType>(string propertyString)
        {
            var existing = fieldChainValidators.Find(x => x.Matches(propertyString));

            if (existing is IFieldDescriptor<TValidationType, TFieldType> converted)
            {
                return converted;
            }

            var validator = new FieldDescriptor<TValidationType, TFieldType>(this, propertyString);
            fieldChainValidators.Add(validator);

            return validator;
        }

        public void AddFieldDescriptor(IFieldDescriptor<TValidationType> fieldDescriptor)
        {
            fieldChainValidators.Add(fieldDescriptor);
        }

        //public virtual AbstractValidator<TFieldType> CreateFieldObjectValidator<TFieldType>(Expression<Func<TValidationType, TFieldType>> property)
        //{
        //    return new AbstractValidator<TFieldType>();
        //}

        public virtual List<IFieldDescriptor<TValidationType>> GetFieldValidators()
        {
            return fieldChainValidators;
        }

        //public virtual List<IChildScope<TValidationType>> GetChildScopes()
        //{
        //    return ChildScopes;
        //}

        //public virtual List<IFieldScope> GetFieldScopes()
        //{
        //    return FieldScopes;
        //}

        public virtual void Include(ValidationScope<TValidationType> scope)
        {
            foreach (var fieldvalidation in scope.GetFieldValidators())
            {
                var copied = false;
                foreach (var currentFieldValidation in GetFieldValidators())
                {
                    if (currentFieldValidation.Property == fieldvalidation.Property)
                    {
                        copied = true;

                        foreach (var validation in fieldvalidation.GetValidations())
                        {
                            currentFieldValidation.AddValidator(validation);
                        }
                        break;
                    }
                }

                if (!copied)
                {
                    fieldChainValidators.Add(fieldvalidation);
                }
            }

            foreach (var validatableObject in scope.validationObjects)
            {
                validationObjects.AddLast(validatableObject);
            }


            //foreach(var childScope in scope.GetChildScopes())
            //{
            //    this.AddChildScope(childScope);
            //}
        }

        //public virtual void Activate(IValidationContext context, TValidationType instance)
        //{
        //    // No Activations required on this scope
        //}

        //public virtual void ActivateToDescribe(IValidationContext context)
        //{
        //    // No Activations required on this scope
        //}

        //public virtual string? Description { get; }
    }

    public class ChildValidationScope<TValidationType> : ValidationScope<TValidationType>
    {
        private readonly string parentProperty;

        public ChildValidationScope(string parentProperty)
        {
            this.parentProperty = parentProperty;
        }

        public override IFieldDescriptor<TValidationType, TFieldType> GetFieldDescriptor<TFieldType>(Expression<Func<TValidationType, TFieldType>> property)
        {
            string propertyString = $"{parentProperty}.{ExpressionUtilities.GetPropertyPath(property)}";

            var existing = fieldChainValidators.Find(x => x.Matches(propertyString));

            if (existing is IFieldDescriptor<TValidationType, TFieldType> converted)
            {
                return converted;
            }

            var validator = new FieldDescriptor<TValidationType, TFieldType>(this, propertyString);
            fieldChainValidators.Add(validator);

            return validator;
        }
    }
}