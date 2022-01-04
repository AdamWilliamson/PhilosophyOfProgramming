using System;

namespace CardPlay.Views.New_File_Dialog
{
    public class CommandObjectBinding<T> : ICommandObjectBinding
    {
        private readonly T component;
        private readonly IMVVMCommand create;
        private Action<T, EventHandler> subscription;
        private Action<T, EventHandler> unsubscription;
        private readonly EventHandler handler;

        public CommandObjectBinding(
            IMVVMCommand create,
            T component,
            Action<T, EventHandler> subscription,
            Action<T, EventHandler> unsubscription,
            EventHandler handler
            )
        {
            this.create = create;
            this.component = component;
            this.subscription = subscription;
            this.unsubscription = unsubscription;
            this.handler = handler;
        }

        public void Subscribe()
        {
            subscription.Invoke(component, handler);
        }

        public void UnSubscribe()
        {
            unsubscription.Invoke(component, handler);
        }
    }
}
