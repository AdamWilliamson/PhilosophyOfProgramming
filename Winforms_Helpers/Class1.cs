using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
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


    public interface IConverter
    {
        object Convert(object input);
    }
    public interface IConverter<TInputType, TOutputType> : IConverter
    {

    }

    public class IntToStringConverter : IConverter<int, string>
    {
        public object Convert(object input)
        {
            return input.ToString();
        }
    }

    public class BindingService : IWinformsBindingService, IDisposable
    {
        private List<IMVVMPropertyBinding> bindings = new List<IMVVMPropertyBinding>();

        public BindingService()
        {
        }

        public BindingService Bind<TViewModel, TComponent, TViewModelType, TComponentType>(
             TViewModel viewModel, Expression<Func<TViewModel, TViewModelType>> viewModelProperty,
             TComponent component, Expression<Func<TComponent, TComponentType>> componentProperty,
             IConverter<TViewModelType, TComponentType> converter = null)
             where TViewModel : INotifyPropertyChanged
        {
            var vmProp = GetPropertyInfo(viewModel, viewModelProperty);
            var componentProp = GetPropertyInfo(component, componentProperty);

            if (typeof(TComponentType) == typeof(ToolStripItemCollection))
            {
                bindings.Add(new ToolbarButtonDropdownPropertyBinding(
                    viewModel,
                    viewModelProperty.Body as MemberExpression,
                    component,
                    componentProperty.Body as MemberExpression,
                    converter
                ));
            }
            else if (typeof(TComponent) == typeof(ListView))
            {
                bindings.Add(new ListViewPropertyBinding(
                    viewModel,
                    viewModelProperty.Body as MemberExpression,
                    component,
                    componentProperty.Body as MemberExpression,
                    converter));
            }
            else if (component is IBindableComponent)
            {
                var boundComponent = component as IBindableComponent;
                boundComponent.DataBindings.Add(componentProp.Name, viewModel, vmProp.Name);
            }
            else if (componentProp.MemberType == MemberTypes.Property)
            {
                bindings.Add(new MVVMPropertyBinding(
                 viewModel,
                 viewModelProperty.Body as MemberExpression,
                 component,
                 componentProperty.Body as MemberExpression,
                 converter
                 ));
            }

            return this;
        }


        /*
public BindingService Bind<TViewModel, TComponent, TType>(
TViewModel viewModel, 
Expression<Func<TViewModel, TType>> viewModelProperty,
TComponent component,
MemberExpression expression)
where TViewModel : INotifyPropertyChanged
{
var vmProp = GetPropertyInfo(viewModel, viewModelProperty);
var componentProp = GetPropertyInfo(viewModel, viewModelProperty);

var binding  = new MVVMPropertyBinding(
   viewModel,
   viewModelProperty,
   component,
   expression
   );
return this;
}
        */
        List<ICommandObjectBinding> CommandBindings = new List<ICommandObjectBinding>();

        public BindingService BindAction<TComponent>(
            IMVVMCommand create,
            TComponent component,
            Action<TComponent, EventHandler> subscription,
            Action<TComponent, EventHandler> unsubscription)
        {
            CommandBindings.Add(
                new CommandObjectBinding<TComponent>(
                    create,
                    component,
                    subscription,
                    unsubscription,
                    (object sender, EventArgs args) =>
                    {
                        create.Execute(args);
                    }));

            return this;
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(
            TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        public void Dispose()
        {
            bindings.ForEach(d => d.Dispose());
            bindings = null;
        }
    }

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
            var c = component as ListView;
            if (c == null) throw new Exception("Not List View");
            var l = obj as IEnumerable;
            if (l == null) throw new Exception("Not a List");

            //SetValue(ComponentExpression, component, obj);
        }

        public override void Dispose()
        {
            return;
        }
    }
}
