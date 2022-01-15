using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations;
using Xunit;

namespace Validations_Tests
{
    public class RunnerTestValidator : AbstractValidator<AllFieldTypesDto>
    {
        public RunnerTestValidator()
        {
            Describe(x => x.Integer).IsEqualTo(1);
        }
    }

    public class ValidationRunner_Tests
    {
        [Fact]
        public void ItExecutesTheValidator_AndHasTheErrors()
        {
            // Arrange
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>> { 
                new RunnerTestValidator(),
            });

            // Act
            var results = runner.Validate(new AllFieldTypesDto());

            // Assert
            results.GetErrors().Keys.Count.Should().Be(1);
        }

        [Fact]
        public void ItExecutesTheDescribe_AndHasTheDescriptions()
        {
            // Arrange
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>> {
                new RunnerTestValidator(),
            });

            // Act
            var results = runner.Describe();

            // Assert
            results.Message.Should().BeNull();
            results.Children.First().Message.Should().Be("Integer");
            results.Children.First().Children.First().Validator.Should().Be("Equal To");
        }
    }
}
