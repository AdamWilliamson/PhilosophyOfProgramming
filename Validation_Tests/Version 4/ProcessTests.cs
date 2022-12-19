using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Validations_Tests.Version_4
{
    // Validation is always grouped by Purpose.  This is the Grouping, the class needs a name denoting Purpose.
    public class ValidationSetGroup
    {
        public List<ValidationSet> ValidationSets { get; set; }
    }

    // Validations can happen on a single field or more.
    //  This is the Set of intrinsically linked validations
    public class ValidationSet
    {
        public string SetIdentifier { get; set; }  // Property, is a poor but eh.  Set Purpose
        public List<ValidationRecord> Records { get; set; }
    }

    // A Record of a validation item.
    public class ValidationRecord
    {
        public ValidationSet OwningSet { get; set; }
        public Rule Rule { get; set; }
        public List<Rule> Rules { get; internal set; }
    }

    // The Rule of a validation.
    public class Rule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValidationErrorMessage { get; internal set; }
        public string ValidationDescription { get; internal set; }
    }

    // The Total of the result of a Validation Of an object
    public class ValidationResult
    {

    }

    // The Total result of a Description request of an object
    public class ValidationDescription
    {

    }


    public interface IPipe
    {
        object Process(object value);

        List<IPipe> NextPipes { get; }
    }

    // A STEP in the validation Process.
    public interface IValidationResultPipe : IPipe {}
    // A STEP in the descipription Process
    public interface IValidationDescriptionPipe : IPipe { }


    public class ValidationSetGroupPipe { }

    public class ViolatedRuleResult { }

    public interface IPipelinePacket { }

    public class Pipeline
    {
        protected List<IPipe> pipes = new();

        public Pipeline AddNextPipe(IPipe pipe)
        {
            pipes.Add(pipe);
            return this;
        }

        public void Process(object input)
        {
            foreach(var pipe in pipes)
            {
                RecursiveProcess(input, pipe);
            }
        }

        protected void RecursiveProcess(object input, IPipe currentPipe)
        { 
            if (input == null && currentPipe != null && currentPipe.NextPipes?.Any() != true)
            {
                return;
            }
            else if (input == null)
            {
                throw new Exception("Previous pipe output was null, creating an invalid connection");
            }
            else if (currentPipe == null)
            {
                throw new ArgumentException("Null when should be a value", "currentPipe");
            }

            var result = currentPipe.Process(input);

            if (result is IEnumerable<IPipelinePacket> packets)
            {
                foreach(var packet in packets)
                {
                    foreach (var nextPipe in currentPipe.NextPipes)
                    {
                        RecursiveProcess(packet, nextPipe);
                    }
                }
            } 
            else if (input is IPipelinePacket)
            {
                foreach (var nextPipe in currentPipe.NextPipes)
                {
                    RecursiveProcess(input, nextPipe);
                }
            }
            else
            {
                throw new Exception("Invalid Pipeline Packet passed between pipes");
            }
        }
    }

    public abstract class Pipe<TInput, TOutput> : IPipe
    {
        public List<IPipe> NextPipes { get; } = new();

        public object? Process(object input)
        {
            if (input is TInput convertedInput)
            {
                var output = ProcessImpl(convertedInput);
            }

            throw new Exception("Pipe Recieved Valid Input");
        }

        protected abstract TOutput ProcessImpl(TInput input);
    }

    public class Parent { }

    public class ParentedRulesDTO
    {
        public List<Parent> Parents { get; } = new();
        public string? Message { get; }
    }

    public class EnumerateValidationSets : Pipe<ValidationSetGroup, ParentedRulesDTO>
    {
        protected override ParentedRulesDTO ProcessImpl(ValidationSetGroup validationGroup)
        {
            return new ParentedRulesDTO();
        }
    }

    public interface IOutputPipe<T> : IPipe 
    {
        public T Output { get; }
    }

    public class ValidationPlumbing
    {
        private Pipeline validationResultPipes;
        private IOutputPipe<ValidationResult> outputValidationResultPipe;
        private Pipeline validationDescriptionPipes = new();
        private IOutputPipe<ValidationDescription> outputValidatioDescriptionPipe;

        public ValidationPlumbing()
        {
            outputValidationResultPipe = null;

            validationResultPipes = new Pipeline()
                .AddNextPipe(new EnumerateValidationSets())
                .AddNextPipe(outputValidationResultPipe);

            validationDescriptionPipes = new Pipeline()
                .AddNextPipe(outputValidatioDescriptionPipe);
        }

        public ValidationResult ProcessValidation(ValidationSetGroup validationSetGroup) 
        {
            ValidationResult result = new();
            //outputValidationResultPipe.Output = result;

            validationResultPipes
                .Process(validationSetGroup);

            //if (item is ValidationResult validationResult)
               // return validationResult;

            throw new Exception("Pipeline Construction Failure");
        }

        public ValidationDescription ProcessDescription(ValidationSetGroup validationSetGroup) 
        {
            ValidationDescription result = new();
            //outputValidatioDescriptionPipe.Output = result;

            validationDescriptionPipes
                .Process(validationSetGroup);

            throw new Exception("Pipeline Construction Failure");
        }
    }


    public class ProcessTests
    {
        [Fact]
        public async Task Test1()
        {
            var validationSetup = new ValidationSetGroup()
            {
                ValidationSets = new List<ValidationSet>
                {
                    new ValidationSet()
                    {
                        SetIdentifier = "AlbumTitle",
                        Records = new List<ValidationRecord>()
                        {
                            new ValidationRecord()
                            {
                                Rules = new List<Rule>{
                                    new Rule()
                                    {
                                        ValidationErrorMessage = "This Rule has been Violated",
                                        ValidationDescription = "This Rule shouldn't be violated"
                                    }
                                }
                            }
                        }
                    }
                }
            };


            var pipeline = new ValidationPlumbing();

            var result = pipeline.ProcessValidation(validationSetup);

            result.Should().NotBeNull();
        }
    }
}
