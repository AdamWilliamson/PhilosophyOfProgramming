using System;
using System.ComponentModel;
using System.Linq.Expressions;
using CardPlay.Utilities;

namespace CardPlay.Views.New_File_Dialog
{
    public class MVVMPropertyBinding : MVVMPropertyBindingBase
    {
        public MVVMPropertyBinding(
            INotifyPropertyChanged viewModel,
            MemberExpression viewMModelExpression,
            object component,
            MemberExpression expression,
            IConverter converter)
            : base(viewModel, viewMModelExpression, component, expression, converter)
        {
            viewModel.PropertyChanged += BoundFunction;
            BoundFunction(this, null);
        }

        protected void BoundFunction(object sender, EventArgs args)
        {
            SetValue(ComponentExpression, Component, GetMemberValue(ViewMModelExpression.Member, ViewModel));
        }

        public override void Dispose()
        {
            ViewModel.PropertyChanged -= BoundFunction;
        }
    }
}
