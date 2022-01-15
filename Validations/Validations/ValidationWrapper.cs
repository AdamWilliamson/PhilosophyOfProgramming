using System.Collections.Generic;
using System.Linq;
using Validations.Internal;

namespace Validations.Validations
{
    public interface IValidationWrapper<T>
    {
        ValidationMessage MessageTemplate { get; }
        List<IValidation> GetValidations(ValidationContext<T> validationContext, T instance);
        List<IValidation> GetAllValidations();
        void SetMessageTemplate(string messageTemplate);
    }

    public class ValidationWrapper<T> : IValidationWrapper<T>
    {
        protected List<IValidation> validations = new();
        protected string? message = null;

        public virtual ValidationMessage MessageTemplate
        {
            get
            {
                if (this.validations == null || validations.Count == 0) 
                {
                    return new ValidationMessage(string.Empty, message);
                }
                else if (validations.Count == 1)
                {
                    var validator = validations.First();
                    return new ValidationMessage(validator.Name, validator.MessageTemplate);
                }
                else
                {
                    var children = validations.Select(x => new ValidationMessage(x.Name, x.MessageTemplate)).ToList();
                    return new ValidationMessage(children);
                }
            }
        }

        public ValidationWrapper(params IValidation[] validations)
        {
            this.validations = validations.ToList();
        }

        public virtual List<IValidation> GetValidations(ValidationContext<T> validationContext, T instance)
        {
            return validations;
        }

        public virtual List<IValidation> GetAllValidations() { return validations; }

        public virtual void SetMessageTemplate(string messageTemplate) { message = messageTemplate; }
    }
}