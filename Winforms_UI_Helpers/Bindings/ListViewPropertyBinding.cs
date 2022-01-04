using System;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CardPlay.Views.New_File_Dialog;

namespace CardPlay.Utilities
{

    public class ListViewPropertyBinding : MVVMPropertyBindingBase
    {
        public ListViewPropertyBinding(
            INotifyPropertyChanged viewModel,
            MemberExpression viewMModelExpression,
            object component,
            MemberExpression expression,
            IConverter converter)
            : base(viewModel, viewMModelExpression, component, expression, converter)
        {
            var obj = GetMemberValue(ViewMModelExpression.Member, ViewModel);
            if (!(component is ListView c)) throw new Exception("Not List View");
            if (!(obj is IEnumerable l)) throw new Exception("Not a List");

            //SetValue(ComponentExpression, component, obj);
        }

        public override void Dispose()
        {
            return;
        }
    }
}
