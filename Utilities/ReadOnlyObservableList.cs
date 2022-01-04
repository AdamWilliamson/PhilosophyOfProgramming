using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Utilities
{
    public class ReadOnlyObservableList<T> : ReadOnlyObservableCollection<T>
    {
        public ReadOnlyObservableList(ObservableCollection<T> list)
            : base(list)
        {
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { base.CollectionChanged += value; }
            remove { base.CollectionChanged -= value; }
        }

        public new event PropertyChangedEventHandler PropertyChanged
        {
            add { base.PropertyChanged += value; }
            remove { base.PropertyChanged -= value; }
        }

        public void ResetBindings()
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
