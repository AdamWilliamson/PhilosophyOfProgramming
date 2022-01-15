using FluentAssertions;
using Validations.Scopes;
using Xunit;

namespace Validations_Tests.Scopes
{
    public class ScopedData_Tests
    {
        [Fact]
        public void ScopedDataCanExecuteTheScopeAndStoresTheDescription()
        {
            var scopeData = new ScopedData<int, int>("Description", (x) => x, 5);

            scopeData.Describe().Should().Be("Description");
            scopeData.GetValue().Should().Be(5);
        }
    }
}
