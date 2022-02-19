using System;
using System.ComponentModel;

namespace Utilities
{
    [Serializable]
    public class ForwardingNotifyPropertyChangedBase : NotifyPropertyChangedBase
    {
        public ForwardingNotifyPropertyChangedBase()
        {
            SetupHooks();
        }

        protected virtual void SetupHooks() { }

        protected virtual void ForwardPropertyChange(object sender, PropertyChangedEventArgs args)
        {
            this.OnPropertyChanged(this, args);
        }

        protected virtual PropertyChangedEventHandler RenamePropForward(string name)
        {
            return (sender, args) => {
                if (sender != null)
                    ForwardPropertyChange(sender, new PropertyChangedEventArgs(name));
            };
        }
    }
}
