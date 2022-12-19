//using Xunit;
//using System.Collections.Generic;
//using Validations;
//using FluentAssertions;
//using Newtonsoft.Json;

//namespace Validation_Tests
//{
//    public class Repository
//    {
//        public TestClassToValidate Get(int number) { return new TestClassToValidate() { Number = number }; }
//    }

//    public class TestClassToValidate
//    {
//        public int Number { get; set; }
//    }

//    public class TestValidator : AbstractValidator<TestClassToValidate>
//    {
//        public TestValidator(Repository repo)
//        {
//            Describe(x => x.Number)
//                .IsEqualTo(0).When("Number == 0", (context, instance) => instance.Number == 0).WithErrorMessage("get yourself some hot bitches")
//                //.If("Only Acceptable if Number is not 5", (instance) => instance.Number == 5, "Cannot Assign if Number is 5").SetVital()
//                //.ElseIf("Something else is true", (instance) => instance.Number == 6, "Cannot Assign if Number is 5")
//                //.If("a different thing is true", (instance) => instance.Number == 5)
//                //.ElseIf("Something else again is true", (instance) => instance.Number == 6)
//                ;
                
//            Scope(
//                (context, instance) => { return repo.Get(instance.Number); },
//                (scopeData) => 
//                {
//                    Describe(x => x.Number)
//                        .IsEqualTo(scopeData.To((data) => data.Number + 1, "Must be equal to number plus 1"));
//                }
//            );

//            When(
//                "When Integer is 5",
//                (context, instance) => instance.Number == 5,
//                () => {
//                    Describe(x => x.Number).IsEqualTo(4);
//                }
//            );

//            ScopeWhen(
//                "When Integer is 5",
//                (context, instance) => instance.Number == 5,
//                (context, instance) => { return repo.Get(instance.Number); },
//                (scopeData) => {
//                    Describe(x => x.Number).IsEqualTo(scopeData.To((data) => data.Number + 1, "Must be equal to number plus 1"));
//                }
//            );

//            //DependOn( new[] {
//            //        x => x.Number,
//            //        x => x.String 
//            //    },
//            //    () => {
//            //        Describe(x => x.String).IsEqualTo(scopeData.To((data) => data.Number + 1, "Must be equal to number plus 1"));
//            //    }
//            //);
//        }
//    }

//    public class UnitTest1
//    {
//        [Fact]
//        public void Test1()
//        {
//            var validatableClass = new TestClassToValidate() { Number = 10 };
//            var validator = new TestValidator(new Repository());
//            var runner = new ValidationRunner<TestClassToValidate>(new List<IScopedValidator<TestClassToValidate>>() { validator });
//            var results = runner.Validate(validatableClass);

//            results.Should().NotBeNull();
//            results.GetErrorsForField("Number").Count.Should().Be(6);
//            results.ContainsError("Number", "get yourself some hot bitches").Should().BeTrue();
//            results.ContainsError("Number", "Stop Being a Bitch").Should().BeTrue();
//        }

//        [Fact]
//        public void Test2()
//        {
//            var validatableClass = new TestClassToValidate() { Number = 10 };
//            var validator = new TestValidator(new Repository());
//            var runner = new ValidationRunner<TestClassToValidate>(new List<IScopedValidator<TestClassToValidate>>() { validator });
//            var description = runner.Describe();

//            var result = JsonConvert.SerializeObject(description);
//            result.Should().Contain("Number");
//            result.Should().Contain("Must equal to {value}");
//        }
//    }
//}