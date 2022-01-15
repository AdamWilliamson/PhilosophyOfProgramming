using FluentAssertions;
using System;
using System.Linq;
using Validations;
using Validations.Internal;
using Xunit;

namespace Validations_Tests
{
    public class ValidationContext_Tests
    {
        [Fact]
        public void ValidationContextAddsErrors()
        {
            // Arrange
            var context = new ValidationContext<AllFieldTypesDto>();

            // Act
            context.AddError("Field", "I am an error for field");
            context.SetCurrentProperty("Field2");
            context.AddError("I am an error for field2");

            // Assert
            context.GetErrors()["Field"].First().Error.Should().Be("I am an error for field");
            context.GetErrors()["Field2"].First().Error.Should().Be("I am an error for field2");
        }

        [Fact]
        public void ValidationContextWiothoutACurrentProperty_ThrowsWhenAddingAnError()
        {
            // Arrange
            var context = new ValidationContext<AllFieldTypesDto>();

            // Act
            context.SetCurrentProperty(null);

            // Assert
            FluentActions.Invoking(() => context.AddError("I am an error for field2")).Should().Throw<InvalidOperationException>();
        }
    }
}
