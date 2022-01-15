using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations;
using Xunit;

namespace Validations_Tests.Validations
{

    public class EqualToTestingValidator : AbstractValidator<AllFieldTypesDto>
    {
        public EqualToTestingValidator()
        {
            Describe(x => x.Integer).IsEqualTo(5);
            Describe(x => x.String).IsEqualTo("5");
            Describe(x => x.Decimal).IsEqualTo(5);
            Describe(x => x.Double).IsEqualTo(5);
            Describe(x => x.Short).IsEqualTo((short)5);
            Describe(x => x.Long).IsEqualTo(5);
            //Describe(x => x.AllFieldTypesDtoChild).IsEqualTo(new AllFieldTypesDto());
        }
    }

    public class EqualityValidation_Tests
    {
        [Fact]
        public void AllComparableBasicFields_AreEqual()
        {
            // Arrange
            var validator = new EqualToTestingValidator();
            var instance = new AllFieldTypesDto()
            {
                Integer = 5,
                String = "5",
                Decimal = 5,
                Double = 5,
                Short = 5,
                Long = 5,
                //AllFieldTypesDtoChild = new AllFieldTypesDto()
            };
            instance.CompareToValue(0);
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>() { validator });

            // Act
            var results = runner.Validate(instance);

            //Assert
            results.GetErrors().Count.Should().Be(0);
        }

        [Fact]
        public void AllComparableBasicFields_AreNotEqual()
        {
            // Arrange
            var validator = new EqualToTestingValidator();
            var instance = new AllFieldTypesDto()
            {
                Integer = 4,
                String = "4",
                Decimal = 4,
                Double = 4,
                Short = 4,
                Long = 4
            };
            instance.CompareToValue(-1);
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>() { validator });

            // Act
            var results = runner.Validate(instance);

            //Assert
            results.GetErrors().Count.Should().Be(6);
        }
    }
}
