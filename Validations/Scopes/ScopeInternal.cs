using System;

namespace Validations.Scopes
{
    public class ScopeInternal<TDataType, TPassThrough>
    {
        public ScopeInternal(){}

        public ScopeInternal(TPassThrough? result)
        {
            Result = result;
        }

        public TPassThrough? Result { get; }

        public ScopedData<TPassThrough, TResponse> To<TResponse>(
            Func<TPassThrough, TResponse> func,
            string description
        )
        {
            return new ScopedData<TPassThrough, TResponse>(
                description: description,
                passThroughFunction: func,
                result: Result
            );
        }
    }
}