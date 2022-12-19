//using System.Collections.Generic;

//namespace Validations.V2;

//interface IValidation { }

//public interface IValidationPackage { }
//class ValidationPackage : IValidationPackage
//{
//    private IFieldDescriptor fieldDescriptor;

//    public ValidationPackage(IFieldDescriptor fieldDescriptor)
//    {
//        this.fieldDescriptor = fieldDescriptor;
//    }
//}


//public interface IFieldDescriptor 
//{
//    void AddValidation(IValidationPackage validationPackage);
//}

//class FieldDescriptor : IFieldDescriptor
//{
//    private readonly ValidationScope validationScope;
//    private string property;
//    //private IValidationPackage validationPackage;

//    public FieldDescriptor(ValidationScope validationScope, string property)
//    {
//        this.validationScope = validationScope;
//        this.property = property;
//    }

//    public void AddValidation(IValidationPackage validationPackage) {
//        this.validationPackage = validationPackage;
//    }
//}

//class EnumeratedFieldDescriptor : IFieldDescriptor
//{
//    private readonly ValidationScope validationScope;
//    private string property;
//    private int index;
//    private IValidationPackage validationPackage;

//    public EnumeratedFieldDescriptor(ValidationScope validationScope, string property, int index)
//    {
//        this.validationScope = validationScope;
//        this.property = property;
//        this.index = index;
//    }

//    public void AddValidation(IValidationPackage validationPackage)
//    {
//        this.validationPackage = validationPackage;
//    }
//}

//internal interface IValidationScope {}

//internal class ValidationScope : IValidationScope
//{
//    private LinkedList<IValidation> ValidationsToExecute = new();
//    public IFieldDescriptor GetFieldDescriptor(string property) { return new FieldDescriptor(this, property);  }
//    public IFieldDescriptor GetFieldDescriptor(string property, int index) { return new EnumeratedFieldDescriptor(this, property, index); }
//}

//public static class FieldValidatorExtensions
//{
//    public static IFieldDescriptor IsEqualTo(this IFieldDescriptor fieldDescriptor)
//    {
//        fieldDescriptor.AddValidation(new ValidationPackage(fieldDescriptor));

//        return fieldDescriptor;
//    }
//}


//public class ValidationRunner<TValidationType>
//{
//    public void Validate(TValidationType instance) { }
//}