﻿using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Validations;
using Xunit;

namespace Validations_Tests.Validations
{
    public class CustomTestingValidator : AbstractValidator<AllFieldTypesDto>
    {
        public CustomTestingValidator()
        {
            Describe(x => x.Integer)
                .Custom((context, instance) => {
                    if (instance == 5)
                    {
                        context.AddError("Something Bad");
                    }
                });
        }
    }

    public class CustomValidation_Tests
    {
        [Fact]
        public void CanRunAndAddsAddsAnError()
        {
            // Arrange
            var validator = new CustomTestingValidator();
            var instance = new AllFieldTypesDto() { Integer = 5 };
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>() { validator });

            // Act
            var results = runner.Validate(instance);

            //Assert
            results.GetErrors().Keys.Should().Contain(nameof(AllFieldTypesDto.Integer));
            results.GetErrors().SelectMany(c => c.Value).SelectMany(v => v.Error).Should().Contain("Something Bad");
        }
    }
}
