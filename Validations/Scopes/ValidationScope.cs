using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Validations.Internal;
using Validations.Validations;

namespace Validations.Scopes
{
    public interface IMessageScope { }

    public interface IScope<TValidationType> : IMessageScope
    {
        void Activate(IValidationContext context, TValidationType instance);
        void ActivateToDescribe(IValidationContext context);
        List<IChildScope<TValidationType>> GetChildScopes();
        ScopeMessage GetValidationDetails();
    }

    public interface IFieldScope 
    {
        string Property { get; }
    }

    public class FieldScope : IFieldScope
    {
        public FieldScope(string property)
        {
            Property = property;
        }

        public string Property { get; }
    }

    public interface IChildScope<TValidationType> : IScope<TValidationType>
    {
        void AddChildScope(IChildScope<TValidationType> scope);
        IScope<TValidationType>? GetParentScope();
        //List<IChildScope<TValidationType>> GetChildScopes();
        //void Activate(IValidationContext context, TValidationType instance);
        //void Describe(IValidationContext context);
        string? Description { get; }
        
    }

    public interface IFieldDescriptorScope<TValidationType>
    {
        IFieldDescriptor<TValidationType, TFieldType> CreateFieldChainValidator<TFieldType>(Expression<Func<TValidationType, TFieldType>> property);
    }

    public interface IValidationScope<TValidationType> : IScope<TValidationType>, IFieldDescriptorScope<TValidationType>
    {
        void AddChildScope(IChildScope<TValidationType> scope);
        void AddFieldScope(IFieldScope fieldScope);
        List<IFieldScope> GetFieldScopes();
        
        string? Description { get; }
        void SetExecutingScope(IScope<TValidationType> scope);
        IScope<TValidationType> GetExecutingScope();
    }

    /// <summary>
    /// The Base scope for each individual Validator Class
    /// It contains functions for assigning validators to fields
    /// Manages child scopes
    /// Should not be supplied to Validator internals.
    /// This is for passing Validator information to the Runner.
    /// </summary>
    /// <typeparam name="TValidationType"></typeparam>
    public class ValidationScope<TValidationType> : IValidationScope<TValidationType>
    {
        private List<IChildScope<TValidationType>> ChildScopes = new();
        private List<IFieldScope> FieldScopes = new();
        private readonly List<IFieldDescriptor<TValidationType>> fieldChainValidators = new();
        IScope<TValidationType> currentScope;

        public ValidationScope()
        {
            this.currentScope = this;
        }

        public IScope<TValidationType>? GetParentScope() { return null; }

        public ScopeMessage GetValidationDetails()
        {
            return new ScopeMessage(nameof(ValidationScope<TValidationType>), Description, GetParentScope()?.GetValidationDetails(), new());
        }

        public void SetExecutingScope(IScope<TValidationType> scope)
        {
            currentScope = scope;
        }

        public IScope<TValidationType> GetExecutingScope()
        {
            return currentScope;
        }

        //public void SetExecutingScope(IValidationScope<TValidationType> scope)
        //{
        //    currentScope = scope;
        //}

        //public IValidationScope<TValidationType> GetExecutingScope()
        //{
        //    return currentScope;
        //}

        public void AddFieldScope(IFieldScope fieldScope) { FieldScopes.Add(fieldScope); }

        public void AddChildScope(IChildScope<TValidationType> scope)
        {
            //if (currentScope == null) ChildScopes.Add(scope);
            //else currentScope.AddScope(scope);

            ChildScopes.Add(scope);
        }

        public IFieldDescriptor<TValidationType, TFieldType> CreateFieldChainValidator<TFieldType>(Expression<Func<TValidationType, TFieldType>> property)
        {
            var existing = fieldChainValidators.Find(x => x.Matches(property));

            if (existing is IFieldDescriptor<TValidationType, TFieldType> converted)
            {
                return converted;
            }

            var validator = new FieldDescriptor<TValidationType, TFieldType>(this, property);
            fieldChainValidators.Add(validator);

            return validator;
        }

        public List<IFieldDescriptor<TValidationType>> GetFieldValidators()
        {
            return fieldChainValidators;
        }

        public List<IChildScope<TValidationType>> GetChildScopes()
        {
            return ChildScopes;
        }

        public List<IFieldScope> GetFieldScopes()
        {
            return FieldScopes;
        }

        public void Include(ValidationScope<TValidationType> scope)
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

            foreach(var childScope in scope.GetChildScopes())
            {
                this.AddChildScope(childScope);
            }
        }

        public void Activate(IValidationContext context, TValidationType instance)
        {
            // No Activations required on this scope
        }

        public void ActivateToDescribe(IValidationContext context)
        {
            // No Activations required on this scope
        }

        public string? Description { get; }
    }
}