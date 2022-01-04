using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Forms;
using CardPlay.Utilities;

namespace CardPlay.Views.New_File_Dialog
{
    public class ToolbarButtonDropdownPropertyBinding : MVVMPropertyBindingBase
    {
        private ToolStripDropDownButton toolBarButton;
        public ToolbarButtonDropdownPropertyBinding(
           INotifyPropertyChanged viewModel,
           MemberExpression viewMModelExpression,
           object component,
           MemberExpression expression,
           IConverter converter)
            : base(viewModel, viewMModelExpression, component, expression, converter)
        {
            toolBarButton = component as ToolStripDropDownButton;
            if (toolBarButton == null) throw new Exception("Binding to not a toolbar Button");
            //toolBarButton.DropDown.ItemClicked += DropDown_ItemClicked;
            viewModel.PropertyChanged += BoundFunction;
            BoundFunction(this, null);
        }

        private void DropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Find Item.
            //Execute it.
        }

        protected void BoundFunction(object sender, EventArgs args)
        {
            //SetValue(ComponentExpression, Component, GetMemberValue(ViewMModelExpression.Member, ViewModel));
            var obj = GetMemberValue(ViewMModelExpression.Member, ViewModel);
            var ListItems = obj as IList<IMVVMCommand>;

            if (ListItems == null && obj != null)
            {
                throw new Exception("Cannont convert type to a list of IMVVMCommands");
            }

            toolBarButton.DropDownItems.Clear();

            foreach (var item in ListItems)
            {
                //var toolitem = WrapItem(item);
                //toolBarButton.DropDownItems.Add(toolitem);
                var button = toolBarButton.DropDownItems.Add(item.Name);
                button.Tag = item;
                button.Click += Button_Click;
            }

        }

        private void Button_Click(object sender, EventArgs e)
        {
            ((sender as ToolStripMenuItem)?.Tag as IMVVMCommand)?.Execute(e);
        }

        public ToolStripButton WrapItem(IMVVMCommand command)
        {
            return new ToolStripButton("", null, (sender, e) => command.Execute(e));
        }

        public override void Dispose()
        {
            ViewModel.PropertyChanged += BoundFunction;
        }
    }
}
