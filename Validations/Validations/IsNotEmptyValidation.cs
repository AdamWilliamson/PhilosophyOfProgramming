using System;
using System.Collections;
using System.Collections.Generic;

namespace Validations.Validations;

public class IsNotEmptyValidation<T> : ValidationBase<T>
{
    public override string Name { get; } = "Is Not Empty";
    public override string DescriptionTemplate { get; } = $"Must not be empty";
    public override string MessageTemplate { get; } = $"Is Not Empty";

	public IsNotEmptyValidation(bool isfatal) : base(isfatal){}

	protected override Dictionary<string, string> GetTokenValues() { return new(); }

    public override bool Test(T? scopedValue, object? instanceValue)
    {
		if (Equals(instanceValue, default(T)))
		{
			return true;
		}

		switch (instanceValue)
		{
			case null:
			case string { Length: 0 }:
			case ICollection { Count: 0 }:
			case Array { Length: 0 }:
			case IEnumerable e when !e.GetEnumerator().MoveNext():
				return true;
		}

		return false;
	}
}
