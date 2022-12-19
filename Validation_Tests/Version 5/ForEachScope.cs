using System;
using System.Collections;
using System.Collections.Generic;

namespace Validations_Tests.Version_5
{
    internal class ForEachIndexedScope<TValidationType, TFieldType> : ScopeBase
    {
        private readonly FieldDescriptor<TValidationType, TFieldType> fieldDescriptor;
        private readonly Action<IFieldDescriptor<TValidationType, TFieldType>> actions;

        public override string Name => "Nothing";

        public ForEachIndexedScope(
            ValidationConstructionStore store,
            FieldDescriptor<TValidationType, TFieldType> fieldDescriptor,
            Action<IFieldDescriptor<TValidationType, TFieldType>> actions
            )
            :base(store)
        {
            this.fieldDescriptor = fieldDescriptor;
            this.actions = actions;
        }

        protected override void InvokeScopeContainer(ValidationConstructionStore store, object? value)
        {
            actions.Invoke(fieldDescriptor);
        }

        protected override void InvokeScopeContainerToDescribe(ValidationConstructionStore store)
        {
            actions.Invoke(fieldDescriptor);
        }
    }

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

                    store.AddItem(
                        thingo, new ForEachIndexedScope<TValidationType, TFieldType>(
                            store, thingo, actions)
                        );
                    //actions?.Invoke(thingo);

                    index++;
                }
            }
        }

        protected override void InvokeScopeContainerToDescribe(ValidationConstructionStore store)
        {
            var thingo = new FieldDescriptor<TValidationType, TFieldType>(
                new IndexedPropertyExpressionToken<TValidationType, TFieldType>(
                    fieldDescriptor.PropertyToken.Expression,
                    fieldDescriptor.PropertyToken.Name + $"[n]",
                    -1
                    ),
                store
                );

            store.AddItem(
                thingo, new ForEachIndexedScope<TValidationType, TFieldType>(
                    store, thingo, actions)
                );
        }
    }
}