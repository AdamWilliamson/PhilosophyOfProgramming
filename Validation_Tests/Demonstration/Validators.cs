using System.Collections.Generic;
using Validations;

namespace Validations_Tests.Demonstration;

public class BasicValidator : AbstractValidator<BasicValidatableObject>
{
    public BasicValidator()
    {
        Describe(x => x.Integer).IsEqualTo(1).IsEqualTo(2);
        Describe(x => x.String).IsVitallyEqualTo(string.Empty);

        Describe(x => x.IntegerList)
            .IsNotNull()
            .ForEach<BasicValidatableObject, List<int>, int>(i => i.IsEqualTo(1));
    }
}

