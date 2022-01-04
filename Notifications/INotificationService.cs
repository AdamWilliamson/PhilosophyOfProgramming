using System;
using System.Collections.Generic;
using System.Reflection;

namespace Notifications
{
    public interface INotificationService
    {
        void Listen<T>(Action<T> action);
        void Notify<T>(T parameter) where T : IDomainEvent;
    }

    public class NotificationService : INotificationService
    {
        public interface IListener
        {
            void Execute(object parameter);
        }

        public class Listener<T> : IListener
        {
            public WeakReference Item;
            public MethodInfo Callback;

            public Listener(Action<T> callback)
            {
                Item = new WeakReference(callback.Target);
                Callback = callback.Method;
            }

            public void Execute(object parameter)
            {
                var obj = Item.Target;
                if (!Item.IsAlive) return;

                try
                {
                    Callback.Invoke(obj, new[] { parameter });
                }
                catch { }
            }
        }

        Dictionary<Type, List<IListener>> Listeners = new Dictionary<Type, List<IListener>>();

        public void Listen<T>(Action<T> action)
        {
            var type = typeof(T);
            if (!Listeners.ContainsKey(type))
                Listeners.Add(type, new List<IListener>());

            Listeners[type].Add(new Listener<T>(action));
        }

        public void Notify<T>(T arg) where T : IDomainEvent
        {
            var type = typeof(T);
            if (!Listeners.ContainsKey(type)) return;

            foreach (var listener in Listeners[type])
            {
                listener.Execute(arg);
            }
        }
    }
}
