using FluentAssertions;
using Validations.Scopes;
using Xunit;

namespace Validations_Tests.Scopes
{
    public class ValidationScope_Tests
    {
        [Fact]
        public void ValidationScopeFunctionsDoWhatYouExpect()
        {
            // Arrange
            var scope = new ValidationScope<AllFieldTypesDto>();
            var scopeToInclude = new ValidationScope<AllFieldTypesDto>();

            // Act
            var fieldChainValidator = scope.CreateFieldChainValidator(x => x.Integer);
            scopeToInclude.CreateFieldChainValidator(x => x.String);

            scope.AddChildScope(new PassThroughChildScope<AllFieldTypesDto, int>(
                scope,
                (x, i) => 4,
                (scopedData) => { }
            ));
            scope.Include(scopeToInclude);

            //Assert
            fieldChainValidator.Should().NotBeNull();
            scope.GetFieldValidators().Count.Should().Be(2);
            scope.GetChildScopes().Should().NotBeEmpty();
        }
    }
}
