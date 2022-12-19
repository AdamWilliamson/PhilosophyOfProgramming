using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Validations.Internal;
using Validations.Scopes;

namespace Validations.Validations;

//public class ForEachValidationWrapper<TOriginValidatorType, TValidationType, TChildObjectType> : IValidationWrapper<TValidationType>
//{
//    protected List<IValidation> validations = new();
//    protected string? message = null;
//    protected IParentScope owningScope;
//    private readonly Action<ChildAbstractValidator<TOriginValidatorType, TChildObjectType>> validationsForEachItem;

//    public virtual ValidationMessage MessageTemplate
//    {
//        get
//        {
//            return new ValidationMessage("null", message, null, null, new());
//        }
//    }

//    public ForEachValidationWrapper(
//        IParentScope owningScope,
//        Action<ChildAbstractValidator<TOriginValidatorType, TChildObjectType>> validationsForEachItem)
//    {
//        this.validations = validations.ToList();
//        this.owningScope = owningScope;
//        this.validationsForEachItem = validationsForEachItem;
//    }

//    public virtual List<IValidation> GetValidations(ValidationContext<TValidationType> validationContext, TValidationType instance)
//    {
//        if (instance is IEnumerable listOfItem){
//            var enumerator = listOfItem.GetEnumerator();
//            int index = 0;

//            while (enumerator.MoveNext())
//            {
//                if (enumerator.Current is TChildObjectType currentValue)
//                {
//                    validationsForEachItem.Invoke(new ChildAbstractValidator<TOriginValidatorType, TChildObjectType>(owningScope, index));
//                }
//                index++;
//            };
//        }
//        return validations;
//    }

//    public virtual List<IValidation> GetAllValidations() { return validations; }

//    public virtual void SetMessageTemplate(string messageTemplate) { message = messageTemplate; }
//    //public IValidationScope<TValidationType> GetOwningScope() {  return owningScope; }

//    public void Activate(IValidationContext context, object instance) {
//        throw new NotImplementedException();
//    }
//    public void Expand(IValidationContext context) { throw new NotImplementedException();  }
//    public ValidationMessage ActivateToDescribe(IValidationContext context)
//    {
//        //var validationpieces = GetAllValidations();
//        //var scopeDescription = owningScope.GetValidationDetails();

//        //var scopeMessages = owningScope.GetValidationDetails();

//        //if (validationpieces.Count == 1)
//        //{
//        //    return validationpieces.First().Describe(context);
//        //}

//        //var messages = new List<ValidationMessage>();
//        //foreach (var validationpiece in validationpieces)
//        //{
//        //    var childMessage = validationpiece.Describe(context);

//        //    if (childMessage != null)
//        //        messages.Add(childMessage);
//        //}

//        //return new ValidationMessage(messages, owningScope.GetValidationDetails());
//        return new ValidationMessage("ForEach", null);
//    }

//    public void MakeVital()
//    {
//        foreach (var validation in GetAllValidations())
//        {
//            validation.IsFatal(true);
//        }
//    }
//}
