using ApprovalTests;
using System.Collections.Generic;
using Validations;
using Validations.Utilities;
using Xunit;

namespace Validations_Tests.Demonstration
{
    public class ToJsonTest
    {
        [Fact]
        public void BasicValidator_Describe_ToJson()
        {
            // Arrange
            var validator = new BasicValidator();
            var runner = new ValidationRunner<BasicValidatableObject>(new List<IValidator<BasicValidatableObject>>() { validator });

            // Act
            var results = runner.Describe();
            var json = new ErrorMessagePrettifier().ToJson(results);

			// Assert
			Approvals.VerifyJson(json);
        }

		[Fact]
		public void BasicValidator_Validate_ToJson()
		{
			// Arrange
			var validator = new BasicValidator();
			var runner = new ValidationRunner<BasicValidatableObject>(new List<IValidator<BasicValidatableObject>>() { validator });

			// Act
			var results = runner.Validate(new BasicValidatableObject());
			var json = new ErrorMessagePrettifier().ToJson(results);

			// Assert
			Approvals.VerifyJson(json);
		}
	}
}
