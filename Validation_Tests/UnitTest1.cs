using Xunit;
using System.Collections.Generic;
using Validations;
using FluentAssertions;
using System.Linq;
using Newtonsoft.Json;

namespace Validation_Tests
{
    public class Repository
    {
        public TestClassToValidate Get(int number) { return new TestClassToValidate() { Number = number }; }
    }

    public class TestClassToValidate
    {
        public int Number { get; set; }
    }

    public class TestValidator : AbstractValidator<TestClassToValidate>
    {
        public TestValidator(Repository repo)
        {
            //Describe(x => x.Number)
            //    .IsEqualTo(0).When((context, instance) => instance.Number == 0);
            //Describe(x => x.Number)
            //    .IsEqualTo(0).WithMessage("get yourself some hot bitches");

            Include(new TestValidatorToInclude(repo));

            Scope(
                (context, instance) => { return repo.Get(instance.Number); },
                (scopeData) => 
                {
                    //Describe(x => x.Number).IsEqualTo(5);
                    Describe(x => x.Number)
                        .IsEqualTo(
                            scopeData.To(
                                (data) => data.Number + 1, 
                                "Must be equal to number plus 1"
                            )
                        );
                }
            );
        }
    }

    public class TestValidatorToInclude : AbstractValidator<TestClassToValidate>
    {
        public TestValidatorToInclude(Repository repo)
        {
            //Describe(x => x.Number)
            //    .IsEqualTo(0);
            //Describe(x => x.Number)
            //    .IsVitallyEqualTo(0);
            //Describe(x => x.Number)
            //    .IsEqualTo(0);
        }
    }

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var validatableClass = new TestClassToValidate() { Number = 10 };
            var validator = new TestValidator(new Repository());
            var runner = new ValidationRunner<TestClassToValidate>(new List<IValidator<TestClassToValidate>>() { validator });
            var results = runner.Validate(validatableClass);

            results.Should().NotBeNull();
            results.GetErrorsForField("Number").Count.Should().Be(6);
            results.ContainsError("Number", "get yourself some hot bitches").Should().BeTrue();
            results.ContainsError("Number", "Stop Being a Bitch").Should().BeTrue();
        }

        [Fact]
        public void Test2()
        {
            var validatableClass = new TestClassToValidate() { Number = 10 };
            var validator = new TestValidator(new Repository());
            var runner = new ValidationRunner<TestClassToValidate>(new List<IValidator<TestClassToValidate>>() { validator });
            var description = runner.Describe();

            var result = JsonConvert.SerializeObject(description);
            result.Should().Contain("Number");
            result.Should().Contain("Must equal to {value}");
        }
    }

    public static class ValidationTestExtensions
    {
        public static List<ValidationError> GetErrorsForField(this ValidationResult? results, string fieldName)
        {
            return results?.GetErrors()[fieldName] ?? new List<ValidationError>();
        }

        public static bool ContainsError(this ValidationResult? results, string fieldName, string error)
        {
            return results?.GetErrorsForField(fieldName)?.Any(e => e.Error == error) ?? false;
        }
    }
}