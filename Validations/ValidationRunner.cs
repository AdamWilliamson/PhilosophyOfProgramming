//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Utilities;
//using Validations.Internal;
//using Validations.Scopes;

//namespace Validations
//{
//    public interface IValidationRunner<TValidationType>
//    {
//        ValidationResult Validate(TValidationType obj);
//        ValidationMessage Describe();
//    }

//    public class ValidationRunner<TValidationType> : IValidationRunner<TValidationType>
//    {
//        private List<IScopedValidator<TValidationType>> Validators { get; } = new();

//        public ValidationRunner(List<IScopedValidator<TValidationType>> validators)
//        {
//            Validators = validators;
//        }

//        public ValidationResult Validate(TValidationType instance)
//        {
//            ArgumentNullException.ThrowIfNull(instance);

//            var context = new ValidationContext<TValidationType>();
            
//            var fieldValidations = new List<List<KeyValuePair<string, List<ValidationError>>>>();

//            foreach (var validator in Validators)
//            {
//                var scope = validator.GetScope();
//                var validationObjects = scope.Expand(context, instance);

//                foreach(var validationObject in validationObjects)
//                {
//                    validationObject.Activate(context, instance);
//                }



//                //var validationObjects = scope.GetValidationObjects();

                
//            //    foreach (var childScope in scope.GetChildScopes())
//            //    {
//            //        //context.PushScope(childScope);

//            //        RecurseScopes(context, scope, childScope, /*instance,*/ (currentScope) => currentScope.Activate(context, instance));

//            //        //context.PopScope();
//            //    }

//            //    fieldValidations.Add(GetErrors(instance, context, scope))
//            //        ;
//            }

//            return new ValidationResult(
//                fieldValidations
//                .SelectMany(x => x)
//                .ToLookup(pair => pair.Key, pair => pair.Value)
//                .ToDictionary(group => group.Key, group => group.First())
//                .Select(x => new FieldErrors() { Property = x.Key, Errors = x.Value })
//                .ToList()
//            );
//        }

//        //private void RecurseScopes(
//        //    ValidationContext<TValidationType> context, 
//        //    ValidationScope<TValidationType> owningScope,
//        //    IScope<TValidationType> currentScope,
//        //    //TValidationType? instance,
//        //    Action<IScope<TValidationType>> action)
//        //{
//        //    //owningScope.SetExecutingScope(currentScope);
//        //    owningScope.SetExecutingScope(currentScope);
//        //    //currentScope.Activate(context, instance);
//        //    action.Invoke(currentScope);

//        //    foreach (var childScope in currentScope.GetChildScopes())
//        //    {
//        //        //context.PushScope(childScope);
//        //        //if (!string.IsNullOrEmpty(childScope.Description))
//        //        //{
//        //        //    context.PushScopeMessage(childScope.Description);
//        //        //}
//        //        RecurseScopes(context, owningScope, childScope, /*instance,*/ action);
//        //        //if (!string.IsNullOrEmpty(childScope.Description))
//        //        //{
//        //        //    context.PopScopeMessage();
//        //        //}
//        //        //context.PopScope();
//        //    }
//        //}

//        private List<KeyValuePair<string, List<ValidationError>>> GetErrors(TValidationType obj, ValidationContext<TValidationType> context, ValidationScope<TValidationType> scope)
//        {
//            if (obj is null) throw new ArgumentNullException(nameof(obj));
//            if (context is null) throw new ArgumentNullException(nameof(context));
//            if (scope is null) throw new ArgumentNullException(nameof(scope));

//            var fieldValidators = scope.GetFieldValidators();
//            var fieldValidations = new List<KeyValuePair<string, List<ValidationError>>>();

//            foreach (var fieldValidator in fieldValidators)
//            {
//                context.SetCurrentProperty(fieldValidator.Property);
                
//                var fieldValue = fieldValidator.GetPropertyValue(obj);

//                var validations = fieldValidator.GetValidations();
//                var error = new KeyValuePair<string, List<ValidationError>>(fieldValidator.Property, new List<ValidationError>());// (Property: fieldValidator.Property, Errors: new List<ValidationError>());

//                foreach (var validation in validations)
//                {
//                    foreach (var validationpieces in validation.GetValidations(context, obj))
//                    {
//                        var res = validationpieces.Validate(context, fieldValue);
//                        if (res != null)
//                            error.Value.Add(res);
//                    }

//                    if (error.Value.Any(err => err.IsFatal)) { break; }
//                }
//                fieldValidations.Add(error);
//            }

//            return fieldValidations;
//        }

//        public ValidationMessage Describe()
//        {
//            var context = new ValidationContext<TValidationType>();
//            var scopeMessages = new List<ValidationMessage>();

//            foreach (var validator in Validators)
//            {
//                var scope = validator.GetScope();

//                //foreach (var childScope in scope.GetChildScopes())
//                //{
//                //    RecurseScopes(context, scope, childScope, (currentScope) => currentScope.ActivateToDescribe(context));
//                //}
//                scopeMessages.AddRange(GetDescriptions(context, scope));
//            }

//            return new ValidationMessage(scopeMessages, null);
//        }

//        private List<ValidationMessage> GetDescriptions(ValidationContext<TValidationType> context, ValidationScope<TValidationType> scope)
//        {
//            if (context is null) throw new ArgumentNullException(nameof(context));
//            if (scope is null) throw new ArgumentNullException(nameof(scope));
            
//            var messages = new List<ValidationMessage>();

//            var fieldValidators = scope.GetFieldValidators();
//            foreach (var fieldValidator in fieldValidators)
//            {
//                var fieldMessageGroup = new ValidationMessage("field", fieldValidator.Property, null, new());
//                messages.Add(fieldMessageGroup);

//                context.SetCurrentProperty(fieldValidator.Property);

//                var validations = fieldValidator.GetValidations();
//                foreach (var validation in validations)
//                {
//                    //foreach (var validationpieces in validation.GetAllValidations())
//                    //{
//                    //    var validationMessage = validation.Describe(context);
                        
//                    //    fieldMessageGroup.Children.Add(validationMessage);
//                    //}
//                }
//            }

//            return messages;
//        }
//    }
//}