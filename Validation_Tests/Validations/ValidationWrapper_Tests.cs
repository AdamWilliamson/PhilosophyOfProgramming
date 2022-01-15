using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations;
using Validations.Internal;
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
            var validationComponent = new EqualityValidation(5, false);
            var wrapper = new ValidationWrapper<AllFieldTypesDto>(validationComponent);
            var validationContext = new ValidationContext<AllFieldTypesDto>();
            var instance = new AllFieldTypesDto();

            // Act
            // Assert
            wrapper.MessageTemplate.Message.Should().Be(validationComponent.MessageTemplate);
            wrapper.GetAllValidations().Count.Should().Be(1);
            wrapper.GetValidations(validationContext, instance).Count.Should().Be(1);
        }

        [Fact]
        public void OverridingTheMessageTemplate_Works()
        {
            // Arrange
            var wrapper = new ValidationWrapper<AllFieldTypesDto>();

            // Act
            wrapper.SetMessageTemplate("I am the new template");

            // Assert
            wrapper.MessageTemplate.Message.Should().Be("I am the new template");
        }
    }
}
