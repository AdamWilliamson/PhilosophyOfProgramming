using System;
using System.Collections.Generic;

namespace Validations.Validations;

public class IsFunctionTestValidation<TResult> : ValidationBase<TResult>
{
    public override string Name { get; } = "Is Function Test";
    private readonly string _DescriptionTemplate;
    public override string DescriptionTemplate { get { return _DescriptionTemplate; } }
    private readonly string _MessageTemplate;
    public override string MessageTemplate { get { return _MessageTemplate; } }
    public Func<TResult, bool>? TestFunction { get; }

    public IsFunctionTestValidation(Func<TResult, bool> testFunction, string description, string error) : base(false)
    {
        TestFunction = testFunction;
        _DescriptionTemplate = description;
        _MessageTemplate = error;
    }

    protected override Dictionary<string, string> GetTokenValues() { return new(); }

    public override bool Test(TResult? scopedValue, object? instanceValue)
    {
        var value = Equals(scopedValue, default) ;

        if (instanceValue is not TResult result) { return false; }

        if (TestFunction != null)
        {
            return TestFunction(result);
        }

        return false;
    }
}