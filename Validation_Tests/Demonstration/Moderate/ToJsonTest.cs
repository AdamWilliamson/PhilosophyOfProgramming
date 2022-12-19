//using ApprovalTests;
//using System.Collections.Generic;
//using Validations;
//using Validations.Utilities;
//using Xunit;

//namespace Validations_Tests.Demonstration.Moderate
//{
//    public class ToJsonTest
//    {
//        [Fact]
//        public void AlbumValidator_Describe_ToJson()
//        {
//            // Arrange
//            var validator = new AlbumValidator();
//            var runner = new ValidationRunner<Album>(new List<IScopedValidator<Album>>() { validator });

//            // Act
//            var results = runner.Describe();
//            var json = new ErrorMessagePrettifier().ToJson(results);

//            // Assert
//            Approvals.VerifyJson(json);
//        }

//        [Fact]
//        public void BasicValidator_Validate_ToJson()
//        {
//            // Arrange
//            var validator = new AlbumValidator();
//            var runner = new ValidationRunner<Album>(new List<IScopedValidator<Album>>() { validator });

//            // Act
//            var results = runner.Validate(new Album());
//            var json = new ErrorMessagePrettifier().ToJson(results);

//            // Assert
//            Approvals.VerifyJson(json);
//        }
//    }
//}
