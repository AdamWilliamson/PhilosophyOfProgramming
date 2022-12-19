using System;
using System.Collections;
using System.Collections.Generic;

namespace Validations_Tests.Version_5
{
    internal class ForEachScope<TValidationType, TFieldType> : ScopeBase
    {
        private readonly IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> fieldDescriptor;
        private Action<IFieldDescriptor<TValidationType, TFieldType>> actions;

        public ForEachScope(
            IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> fieldDescriptor,
            Action<IFieldDescriptor<TValidationType, TFieldType>> actions)
            :base(fieldDescriptor.Store)
        {
            this.fieldDescriptor = fieldDescriptor;
            this.actions = actions;
        }

        public override string Name => nameof(ForEachScope<TValidationType, TFieldType>);

        protected override void InvokeScopeContainer(ValidationConstructionStore store, object? value)
        {
            if (value == null) { return; }
            if (value is IEnumerable<TFieldType> list)
            {
                int index = 0;
                foreach (var item in list)
                {
                    var thingo = new FieldDescriptor<TValidationType, TFieldType>(
                        new IndexedPropertyExpressionToken<TValidationType, TFieldType>(
                            fieldDescriptor.PropertyToken.Expression,
                            fieldDescriptor.PropertyToken.Name + $"[{index}]",
                            index
                            ),
                        //fieldDescriptor.ParentScope,
                        store//fieldDescriptor.Store,
                        );
                    actions?.Invoke(thingo);

                    index++;
                }
            }
        }
    }
}