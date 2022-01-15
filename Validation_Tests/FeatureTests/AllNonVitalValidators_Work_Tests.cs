using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using System.Linq;
using Validations;
using Xunit;

namespace Validations_Tests
{
    public class AllNonVitalValidators_Validator : AbstractValidator<AllFieldTypesDto> 
    {
        public AllNonVitalValidators_Validator() 
        {
            // Non-Class Fields
            Describe(x => x.Integer).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.String).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.Object).AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.Decimal).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.Double).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.Short).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.Long).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();

            // Class Fields
            Describe(x => x.Type).AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.TwoComponentTupple).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();
            //Describe(x => x.AllFieldTypesDtoChild).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();

            // Psuedo Anonymous Fields
            Describe(x => x.TwoComponentNewTupple).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();

            // Lists
            Describe(x => x.AllFieldTypesList).AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.AllFieldTypesLinkedList).AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.AllFieldTypesIEnumerable).AddAllValidatorsRequiringAClassOwner();
            Describe(x => x.AllFieldTypesDictionary).AddAllValidatorsRequiringAClassOwner();

            // Struct
            Describe(x => x.Struct).AddAllValidatorsRequiringAClassOwner();
        }
    }

    public class AllNonVitalValidators_Work_Tests
    {
        [Fact]
        public void AllFields_ShouldContainAtleast1Error()
        {
            //Arrange
            int fieldCount = typeof(AllFieldTypesDto).GetProperties().Length;
            var validatableClass = new AllFieldTypesDto();
            var validator = new AllNonVitalValidators_Validator();
            var runner = new ValidationRunner<AllFieldTypesDto>(new List<IValidator<AllFieldTypesDto>>() { validator });

            // Act
            var results = runner.Validate(validatableClass);

            // Assert
            using (new AssertionScope())
            {
                results.GetErrors().Keys.Count.Should().Be(fieldCount);
                results.GetErrors().Keys.Should().BeEquivalentTo(
                    typeof(AllFieldTypesDto).GetProperties().Select(f => f.Name)
                );
                results.GetErrors().Values.Select(x => x.Count).Should().OnlyContain(x => x > 0);
            }
        }
    }
}
