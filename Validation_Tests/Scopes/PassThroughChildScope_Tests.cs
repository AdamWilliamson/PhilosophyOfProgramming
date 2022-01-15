using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations;
using Validations.Internal;
using Validations.Scopes;
using Xunit;

namespace Validations_Tests.Scopes
{
    public class ValidationType { }

    public class PassThroughChildScope_Tests
    {
        [Fact]
        public void InternalScopePassesthroughDataSuccessfully()
        {
            // Arrange
            ScopedData<int,int>? internalScope = null;
            var passthrough = new PassThroughChildScope<ValidationType, int>(
                (validationType, integer) => { return 5; },
                (scopedInternal) =>
                {
                    internalScope = scopedInternal.To(i => i, "5");
                }
            );

            // Act
            passthrough.Activate(new ValidationContext<ValidationType>(), new ValidationType());

            var result = internalScope?.GetValue();

            // Assert
            result.Should().Be(5);
            internalScope!.Describe().Should().Be("5");
        }
    }
}
