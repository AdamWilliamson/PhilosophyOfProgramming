using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Validations;
using Xunit;

namespace Validations_Tests
{
    public class ScopeWithinScopeValidationsTests : AbstractValidator<AllFieldTypesDto>
    {
        public ScopeWithinScopeValidationsTests()
        {
            Scope(
                (context, instance) => 5,
                (scopedData) => {

                    Scope(
                        (context, instance) => "5",
                        (scopedData) => {
                            Describe(x => x.String).IsEqualTo(scopedData.To(x => x, "5"));
                        }
                    );

                }
            );
        }
    }

    public class MultipleScopesValidationsTests : AbstractValidator<AllFieldTypesDto>
    {
        public MultipleScopesValidationsTests()
        {
            Scope(
                (context, instance) => 5,
                (scopedData) => {
                    Describe(x => x.String).IsEqualTo(scopedData.To(x => x, "5"));
                }
            );

            Scope(
                (context, instance) => "5",
                (scopedData) => {
                    Describe(x => x.String).IsEqualTo(scopedData.To(x => x, "5"));
                }
            );
        }
    }

    public class AbstractValidatorTestValidator : AbstractValidator<AllFieldTypesDto> 
    {
        public AbstractValidatorTestValidator()
        {
            Describe(x => x.Integer);
            Describe(x => x.String).IsEqualTo("1");

            Include(new TestValidatorToInclude());
            Include(new TestValidatorToInclude());
        }
    }

    public class TestValidatorToInclude : AbstractValidator<AllFieldTypesDto>
    {
        public TestValidatorToInclude()
        {
            Describe(x => x.String).IsVitallyEqualTo("2");

            Scope(
                (context, instance) => 5,
                (scopedData) => {
                    Describe(x => x.Object);
                }
            );
        }
    }

    public class SecondTestValidatorToInclude : AbstractValidator<AllFieldTypesDto>
    {
        public SecondTestValidatorToInclude()
        {
            Describe(x => x.String).IsEqualTo("3");

            Scope(
                (context, instance) => 5,
                (scopedData) => {
                    Describe(x => x.Object);
                }
            );
        }
    }

    public class AbstractValidator_Tests
    {
        [Fact]
        public void Include_Works()
        {
            // Arrange
            var validator = new AbstractValidatorTestValidator();
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>()
            {
                validator
            });

            // Act
            var results = runner.Validate(new AllFieldTypesDto() { String = "Not A Number" });

            // Assert
            ((IValidator<AllFieldTypesDto>)validator).GetScope().GetChildScopes().Count.Should().Be(2);
        }

        [Fact]
        public void Include_WithScopes_Works()
        {
            // Arrange
            var validator = new AbstractValidatorTestValidator();
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>()
            {
                validator
            });

            // Act
            var results = runner.Validate(new AllFieldTypesDto() { String = "Not A Number" });

            // Assert
            ((IValidator<AllFieldTypesDto>)validator).GetScope().GetChildScopes().Count.Should().Be(2);
        }

        [Fact]
        public void Include_StopsInOrder()
        {
            // Arrange
            var validator = new AbstractValidatorTestValidator();
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>()
            {
                validator
            });

            // Act
            var results = runner.Validate(new AllFieldTypesDto() { String = "Not A Number" });

            // Assert
            results.FieldErrors.Single(o => o.Property == nameof(AllFieldTypesDto.String)).Errors.Count.Should().Be(2);
        }

        [Fact]
        public void Scope_WithinScope_Works()
        {
            // Arrange
            var validator = new ScopeWithinScopeValidationsTests();
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>()
            {
                validator
            });

            // Act
            var results = runner.Validate(new AllFieldTypesDto() { String = "Not A Number" });

            // Assert
            results.FieldErrors.Single(o => o.Property == nameof(AllFieldTypesDto.String)).Errors.Count.Should().Be(1);
        }

        [Fact]
        public void MultipleScopes_Works()
        {
            // Arrange
            var validator = new MultipleScopesValidationsTests();
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>()
            {
                validator
            });

            // Act
            var results = runner.Validate(new AllFieldTypesDto() { String = "Not A Number" });

            // Assert
            results.FieldErrors.Single(o => o.Property == nameof(AllFieldTypesDto.String)).Errors.Count.Should().Be(2);
        }
    }
}
