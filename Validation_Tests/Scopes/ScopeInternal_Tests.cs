using FluentAssertions;
using Validations.Scopes;
using Xunit;

namespace Validations_Tests.Scopes
{
    public class ScopeInternal_Tests
    {
        [Fact]
        public void IsAbleToGenerateInternalScopedDataThatFunctionsCorrectly()
        {
            var scopeInternal = new ScopeInternal<ValidationType, int>(5);
            var scopedData = scopeInternal.To(x => x, "Description");

            scopedData.Should().NotBeNull();
            scopedData.Describe().Should().Be("Description");
            scopedData.GetValue().Should().Be(5);
        }
    }
}
