using System.Collections.Generic;
using Validations.Internal;
using Validations.Validations;

namespace Validations
{
    public interface IFieldDescriptor<TClassType, TFieldType>
    {
        IFieldDescriptor<TClassType, TFieldType> AddValidator(IValidationWrapper<TClassType> validation);
        IList<IValidationWrapper<TClassType>> GetValidations();
    }

    public sealed class FieldDescriptor<TClassType, TFieldType> : IFieldDescriptor<TClassType, TFieldType>
    {
        private readonly IFieldChainValidator<TClassType> fieldChainValidator;

        public FieldDescriptor(IFieldChainValidator<TClassType> fieldChainValidator)
        {
            this.fieldChainValidator = fieldChainValidator;
        }

        public IFieldDescriptor<TClassType, TFieldType> AddValidator(IValidationWrapper<TClassType> validation) 
        {
            fieldChainValidator.AddValidator(validation);
            return this;
        }

        public IList<IValidationWrapper<TClassType>> GetValidations()
        {
            return fieldChainValidator.GetValidations();
        }
    }
}