//using System.Collections.Generic;
//using Validations.Internal;
//using Validations.Scopes;
//using Validations.Validations;

//namespace Validations
//{
//    //public interface IScopedRunnable<TValidationType>
//    //{
//    //    void SetCurrentScope(IScope<TValidationType> scaope);
//    //}

//    public interface IFieldDescriptor<TValidationType, TFieldType> 
//    {
//        IFieldDescriptor<TValidationType, TFieldType> AddValidator(IValidationWrapper<TValidationType> validation);
//        IList<IValidationWrapper<TValidationType>> GetValidations();
//        IScope<TValidationType> GetCurrentScope();
//    }

//    public sealed class FieldDescriptor<TValidationType, TFieldType> : IFieldDescriptor<TValidationType, TFieldType> ///, IScopedRunnable<TValidationType>
//    {
//        private IScope<TValidationType>? currentScope = null;
//        private readonly IFieldChainValidator<TValidationType> fieldChainValidator;

//        public FieldDescriptor(ValidationScope<TValidationType> parentScope, IFieldChainValidator<TValidationType> fieldChainValidator)
//        {
//            this.currentScope = parentScope;
//            this.fieldChainValidator = fieldChainValidator;
//        }

//        public IFieldDescriptor<TValidationType, TFieldType> AddValidator(IValidationWrapper<TValidationType> validation) 
//        {
//            fieldChainValidator.AddValidator(validation);
//            return this;
//        }

//        public IList<IValidationWrapper<TValidationType>> GetValidations()
//        {
//            return fieldChainValidator.GetValidations();
//        }

//        //public void SetCurrentScope(IScope<TValidationType> currentScope) 
//        //{
//        //    this.currentScope = currentScope;
//        //}

//        public IScope<TValidationType> GetCurrentScope()
//        {
//            if (currentScope == null)
//            {
//                throw new System.Exception();
//            }

//            return currentScope;
//        }
//    }
//}