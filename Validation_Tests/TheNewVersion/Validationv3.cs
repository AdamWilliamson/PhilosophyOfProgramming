//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Validations_Tests.TheNewVersion
//{


//    public class FieldValidationResult { }

//    public interface IFieldValidation 
//    {
//        FieldValidationResult Validate();
//    }

//    public interface IFieldDescriptor { }
//    public interface IFieldDescriptor<TFieldType> : IFieldDescriptor
//    { }

//    public interface IValidationItem 
//    {
//        void Validate(object validatableObject, ValidationError validationError);
//    }

//    public interface IScope<TValidationType>
//    {
//        void Validate(TValidationType validatableObject, ValidationError validationError);
//    }

//    public class AbstractValidator<TValidationType> : IScope<TValidationType>
//    {
//        private List<IValidationItem> ValidationItems = new List<IValidationItem>();

//        public IFieldDescriptor<TFieldType> Describe<TFieldType>(Expression<Func<TValidationType, TFieldType>> expr)
//        {
//            return null;
//        }

//        public void Scope(IScope<TValidationType> childScope)
//        {
//            ValidationItems.AddRange(childScope.GetInterfaceItems());
//        }

//        public List<IValidationItem> GetInterfaceItems()
//        {
//            return ValidationItems;
//        }

//        void IScope<TValidationType>.Validate(TValidationType validatableObject, ValidationError validationError)
//        {
//            if (validatableObject == null) throw new ArgumentNullException(nameof(validatableObject));

//            foreach(var item in ValidationItems)
//            {
//                item.Validate(validatableObject, validationError);
//            }
//        }
//    }

//    public class ValidationError
//    {
//        public string? Error { get; set; }
//        public string? Description { get; set; }
//        public List<ValidationError> ChildErrors { get; } = new();
//    }

//    public class ValidationResult 
//    {
//        public List<ValidationError> Errors { get; } = new();

//        public bool IsError
//        {
//            get { return Errors.Any(); }
//        }
//    }


//    public class ValidationRunner<TValidationType>
//    {
//        private IEnumerable<IValidator> Validators { get; }

//        public ValidationRunner(IEnumerable<IValidator> validators)
//        {
//            Validators = validators;
//        }

//        public ValidationResult Validate(TValidationType validationObject)
//        {
//            var validationResult = new ValidationResult();
    
//            foreach (var validator in Validators)
//            {
//                var validatorErrorCollection = new ValidationError();
//                validator.Validate(validatorErrorCollection);
//                validationResult.Errors.Add(validatorErrorCollection);
//            }

//            return validationResult;
//        }

//    }
//}
