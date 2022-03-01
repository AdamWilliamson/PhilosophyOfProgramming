using System;

namespace Validations.Scopes
{
    public interface IScopedData
    {
        object? GetValue();
        string  Describe();
    }

    public interface IScopedData<TResponse> : IScopedData 
    {
    }

    public class ScopedData<TPassThrough, TResponse> : IScopedData<TResponse>
    {
        private Func<TPassThrough, TResponse> PassThroughFunction { get; }
        private string Description { get; }
        private TPassThrough? Result { get; }

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