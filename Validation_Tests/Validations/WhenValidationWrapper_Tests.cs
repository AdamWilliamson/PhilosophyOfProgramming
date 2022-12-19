//using FluentAssertions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Validations;
//using Xunit;

//namespace Validations_Tests.Validations
//{
//    public class WhenTestingValidator : AbstractValidator<AllFieldTypesDto>
//    {
//        public WhenTestingValidator()
//        {
//            Describe(x => x.Integer).IsEqualTo(6).When("Integer is 5", (context, instance) => instance.Integer == 5);
//        }
//    }

//    public class WhenValidationWrapper_Tests
//    {
//        [Fact]
//        public void TheValidationRunsButIsTrue_WhenTheCheckIstrue()
//        {
//            var validator = new WhenTestingValidator();
//            var instance = new AllFieldTypesDto()
//            {
//                Integer = 5
//            };
//            instance.CompareToValue(0);
//            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IScopedValidator<AllFieldTypesDto>>() { validator });

//            // Act
//            var results = runner.Validate(instance);

//            //Assert
//            results.FieldErrors.Count.Should().Be(1);
//        }

//        [Fact]
//        public void TheValidationDoesNotRunWhenTheCheckIsFalse()
//        {
//            var validator = new WhenTestingValidator();
//            var instance = new AllFieldTypesDto()
//            {
//                Integer = 6
//            };
//            instance.CompareToValue(0);
//            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IScopedValidator<AllFieldTypesDto>>() { validator });

//            // Act
//            var results = runner.Validate(instance);

//            //Assert
//            results.FieldErrors.Count.Should().Be(0);
//        }
//    }
//}
