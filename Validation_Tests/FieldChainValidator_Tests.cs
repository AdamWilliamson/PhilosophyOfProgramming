using FluentAssertions;
using System;
using Validations;
using Validations.Internal;
using Validations.Scopes;
using Validations.Validations;
using Xunit;

namespace Validations_Tests
{
    public class TestValidation : ValidationBase<IComparable>
    {
        public override string Name { get; } = "Test";
        public override string DescriptionTemplate { get; } = "Is Same";
        public override string MessageTemplate { get; } = "{value} is not equal to {value}";
        public int Value { get; }

        public TestValidation(int value, bool isfatal) : base(isfatal)
        {
            Value = value;
        }

        public TestValidation(IScopedData value, bool isfatal) : base(value, isfatal) { }

        public override bool Test(IComparable? internalValue, object? instanceValue)
        {
            return internalValue == null && instanceValue == null || internalValue?.Equals(instanceValue) == true;
        }
    }

    public class FieldChainValidator_Tests
    {
        [Fact]
        public void AddingAValidator_Works()
        {
            // Arrange
            var fieldChainValidator = new FieldChainValidator<AllFieldTypesDto, int>(x => x.Integer);

            // Act
            fieldChainValidator.AddValidator(new ValidationWrapper<AllFieldTypesDto>(new TestValidation(5, false)));

            // Assert
            fieldChainValidator.GetValidations().Count.Should().Be(1);
        }

        [Fact]
        public void MatchingAValidator_Works()
        {
            // Arrange
            var fieldChainValidator = new FieldChainValidator<AllFieldTypesDto, int>(x => x.Integer);

            // Act
            // Assert
            fieldChainValidator.Matches(x => x.Integer).Should().BeTrue();
        }

        //[Fact]
        //public void MatchingAValidatorForASubObject_Works()
        //{
        //    // Arrange
        //    var fieldChainValidator = new FieldChainValidator<AllFieldTypesDto, int>(x => x.AllFieldTypesDtoChild.Integer);

        //    // Act
        //    // Assert
        //    fieldChainValidator.Matches(x => x.Integer).Should().BeFalse();
        //    fieldChainValidator.Matches(x => x.AllFieldTypesDtoChild.Integer).Should().BeTrue();
        //}
    }
}
