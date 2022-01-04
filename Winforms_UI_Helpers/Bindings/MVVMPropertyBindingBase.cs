using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using CardPlay.Utilities;
using Utilities;

namespace CardPlay.Views.New_File_Dialog
{
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
}
