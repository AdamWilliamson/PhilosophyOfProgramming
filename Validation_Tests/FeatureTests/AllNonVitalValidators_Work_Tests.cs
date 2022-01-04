using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using Validations;
using Xunit;

namespace Validations_Tests
{
    public struct FieldTestStruct {
        public int Integer { get; }
    }

    public class AllFieldTypesDto : IComparable
    {
        public int Integer { get; } = 0;
        public string String { get; } = String.Empty;
        public object Object { get; } = new object();
        public decimal Decimal { get; } = decimal.MaxValue;
        public double Double { get; } = double.MaxValue;
        public short Short { get; } = short.MaxValue;
        public long Long { get; } = long.MaxValue;
        public Type Type { get; } = typeof(AllFieldTypesDto);
        public Tuple<int, int> TwoComponentTupple { get; } = Tuple.Create(1, 2);
        public (int one, int two) TwoComponentNewTupple { get; } = (0, 0);
        public AllFieldTypesDto AllFieldTypesDtoChild { get; } = new();

        public List<AllFieldTypesDto> AllFieldTypesList { get; } = new();
        public LinkedList<AllFieldTypesDto> AllFieldTypesLinkedList { get; } = new ();
        public IEnumerable<AllFieldTypesDto> AllFieldTypesIEnumerable { get; } = new List<AllFieldTypesDto>();
        public Dictionary<string, AllFieldTypesDto> AllFieldTypesDictionary { get; } = new();
        public FieldTestStruct Struct { get; } = new();

        public int CompareTo(object? obj)
        {
            return -1;
        }
    }

    public static class AllFieldsExtension
    {
        public static FieldChainValidator<T, TResult> AddAllIComparableValidators<T, TResult>(this FieldChainValidator<T, TResult> describe)
            where TResult: IComparable
        {
            return describe.IsEqualTo(default(TResult));
        }

        public static FieldChainValidator<T, TResult> AddAllValidatorsRequiringAClassOwner<T, TResult>(this FieldChainValidator<T, TResult> describe)
            where T: class
        {
            return describe.Custom((context, instance) => {
                context.AddError("Bad thing happened");
            });
        }
    }

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
            Describe(x => x.AllFieldTypesDtoChild).AddAllIComparableValidators().AddAllValidatorsRequiringAClassOwner();

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
