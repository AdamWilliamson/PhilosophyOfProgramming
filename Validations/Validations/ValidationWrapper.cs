using System.Collections.Generic;
using System.Linq;

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
        protected string message = "";

        public virtual ValidationMessage MessageTemplate
        {
            get
            {
                var msg = message;
                var children = new List<ValidationMessage>();

                if (validations.Count == 1)
                {
                    msg = validations.First().MessageTemplate;
                }
                else
                {
                    children = validations.Select(x => new ValidationMessage(x.MessageTemplate)).ToList();
                }

                return new ValidationMessage(message, children);
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