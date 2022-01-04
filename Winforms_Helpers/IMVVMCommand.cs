using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using CardPlay.Utilities;

namespace CardPlay.Views.New_File_Dialog
{
    public interface IMVVMCommand
    {
        void Execute(EventArgs e);
        string Name { get; }
    }

    public class MVVMCommand : IMVVMCommand
    {
        Action<EventArgs> action;

        public MVVMCommand(Action<EventArgs> action)
        {
            this.action = action;
        }

        public MVVMCommand(string name, Action<EventArgs> action)
            : this(action)
        {
            Name = name;
        }

        public string Name { get; protected set; }

        public void Execute(EventArgs e)
        {
            action.Invoke(e);
        }

        public static implicit operator MVVMCommand(Action<EventArgs> action)
        {
            return new MVVMCommand(action);
        }
    }

    public interface IMVVMPropertyBinding : IDisposable
    {

    }

    public abstract class MVVMPropertyBindingBase : IMVVMPropertyBinding
    {
        protected object Component;
        protected MemberExpression ComponentExpression;
        protected readonly IConverter Converter;
        protected MemberExpression ViewMModelExpression;
        protected INotifyPropertyChanged ViewModel;

        public MVVMPropertyBindingBase(
            INotifyPropertyChanged viewModel,
            MemberExpression viewMModelExpression,
            object component,
            MemberExpression expression,
            IConverter converter)
        {
            ThrowIf.Arguments.AreNull(
                () => viewModel,
                () => viewMModelExpression,
                () => component,
                () => expression);

            this.ViewModel = viewModel;
            this.ViewMModelExpression = viewMModelExpression;
            this.Component = component;
            this.ComponentExpression = expression;
            Converter = converter;
        }

        protected object GetMemberValue(MemberInfo member, object target)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(target);
                case MemberTypes.Property:
                    try
                    {
                        return ((PropertyInfo)member).GetValue(target, null);
                    }
                    catch (TargetParameterCountException e)
                    {
                        throw new ArgumentException("MemberInfo has index parameters", "member", e);
                    }
                default:
                    throw new ArgumentException("MemberInfo is not of type FieldInfo or PropertyInfo", "member");
            }
        }


        protected void SetValue(MemberExpression member, object target, object value)
        {
            if (member != null)
            {
                var property = member.Member as PropertyInfo;
                if (property != null)
                {
                    if (Converter != null)
                        value = Converter.Convert(value);
                    property.SetValue(target, value, null);
                }
            }
        }


        public abstract void Dispose();
    }

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
                throw new DomainException("Cannont convert type to a list of IMVVMCommands");
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
