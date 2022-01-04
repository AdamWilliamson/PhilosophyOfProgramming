using System;
using System.ComponentModel;
using System.Linq.Expressions;
using CardPlay.Views.New_File_Dialog;

namespace CardPlay.Utilities
{
    public interface IWinformsBindingService : IDisposable
    {
        BindingService Bind<TViewModel, TComponent, TViewModelType, TComponentType>(
            TViewModel viewModel, Expression<Func<TViewModel, TViewModelType>> viewModelProperty,
            TComponent component, Expression<Func<TComponent, TComponentType>> componentProperty,
            IConverter<TViewModelType, TComponentType> converter = null)
            where TViewModel : INotifyPropertyChanged;

        BindingService BindAction<TComponent>(
            IMVVMCommand create,
            TComponent component,
            Action<TComponent, EventHandler> subscription,
            Action<TComponent, EventHandler> unsubscription);
    }
}
