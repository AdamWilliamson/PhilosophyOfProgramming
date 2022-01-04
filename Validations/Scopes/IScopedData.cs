using System;

namespace Validations.Scopes
{
    public interface IScopedData
    {
        object? GetValue();
        string  Describe();
    }

    public class ScopedData<TPassThrough, TResponse> : IScopedData
    {
        public Func<TPassThrough, TResponse> PassThroughFunction { get; set; }
        public string Description { get; internal set; }
        public TPassThrough? Result { get; internal set; }

        public ScopedData(
            string description,
            Func<TPassThrough, TResponse> passThroughFunction,
            TPassThrough? result
        )
        {
            Description = description;
            PassThroughFunction = passThroughFunction;
            Result = result;
        }

        public object? GetValue()
        {
            if (Result == null) return null;

            return PassThroughFunction.Invoke(Result);
        }

        public string Describe()
        {
            return Description;
        }
    }
}