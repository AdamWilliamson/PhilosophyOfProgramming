using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;
using Xunit;

namespace Validations_Tests.Validations
{
    public class ValidationWrapper_Tests
    {
        [Fact]
        public void AllFieldsReturnTheCorrectValues()
        {
            // Arrange
            var owningScope = new ValidationScope<AllFieldTypesDto>();
            var validationComponent = new IsEqualToValidation(5, false);
            var wrapper = new ValidationWrapper<AllFieldTypesDto>(owningScope, validationComponent);
            var validationContext = new ValidationContext<AllFieldTypesDto>();
            var instance = new AllFieldTypesDto();

            // Act
            // Assert
            wrapper.MessageTemplate.Error.Should().Be(validationComponent.MessageTemplate);
            wrapper.GetAllValidations().Count.Should().Be(1);
            wrapper.GetValidations(validationContext, instance).Count.Should().Be(1);
        }

        [Fact]
        public void OverridingTheMessageTemplate_Works()
        {
            // Arrange
            var owningScope = new ValidationScope<AllFieldTypesDto>();
            var wrapper = new ValidationWrapper<AllFieldTypesDto>(owningScope);

            // Act
            wrapper.SetMessageTemplate("I am the new template");

            // Assert
            wrapper.MessageTemplate.Error.Should().Be("I am the new template");
        }
    }
}
