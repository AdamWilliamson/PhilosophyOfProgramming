using System;
using System.Collections.Generic;

namespace POP.Builders
{
    public interface IBuilder<T>
        where T: class
    {
        T Build();
    }

    public abstract class Builder<T>: IBuilder<T>
        where T : class
    {
        List<Action<T>> actions = new List<Action<T>>();

        protected abstract T BuildObject();

        public T Build()
        {
            var obj = BuildObject();
            actions.ForEach(a => a.Invoke(obj));

            return obj;
        }

        public Builder<T> With(Action<T> action)
        {
            actions.Add(action);
            return this;
        }
    }
}
