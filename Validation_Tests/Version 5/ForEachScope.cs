using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Validations_Tests.Version_5;
public class ForEachValidationActionResult : ValidationActionResult
{
    private readonly string propertyTest;

    public ForEachValidationActionResult(
        string propertyTest
    )
        :base(nameof(ForEachValidationActionResult),true, "", new())
    {
        this.propertyTest = propertyTest;
    }

    public override List<string>? GetFailedDependantFields(string currentProperty, ValidationResult currentValidationResult)
    {
        currentProperty = currentProperty.Substring(
            0, 
            currentProperty.LastIndexOf(propertyTest) + propertyTest.Length
        );

        if (currentValidationResult?.Results
            ?.Any(r => r.Property.StartsWith(currentProperty)) == true)
        {
            return new List<string>() { currentProperty };
        }
        return null;
    }
}

public class VitallyForEachValidation : ValidationComponentBase
{
    private readonly string property;

    public VitallyForEachValidation(string property)
    {
        this.property = property;
    }

    public override string Name { get; } = "Depends On";
    public override string DescriptionTemplate { get; protected set; } = $"";
    public override string ErrorTemplate { get; protected set; } = $"";

    public override ValidationActionResult Validate(object? value)
    {
        return new ForEachValidationActionResult(property);
    }

    public override DescribeActionResult Describe()
    {
        return new DescribeActionResult(
            validator: nameof(VitallyForEachValidation),
            message: DescriptionTemplate,
            new List<KeyValuePair<string, string>>()
        );
    }
}

internal class ForEachIndexedScope<TValidationType, TFieldType> : ScopeBase
{
    public override bool IgnoreScope => true;
    private readonly FieldDescriptor<IEnumerable<TFieldType>, TFieldType> fieldDescriptor;
    private readonly Action<IFieldDescriptor<IEnumerable<TFieldType>, TFieldType>> actions;

    public override string Name => "Nothing";

    public ForEachIndexedScope(
        ValidationConstructionStore store,
        FieldDescriptor<IEnumerable<TFieldType>, TFieldType> fieldDescriptor,
        Action<IFieldDescriptor<IEnumerable<TFieldType>, TFieldType>> actions
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
    private Action<IFieldDescriptor<IEnumerable<TFieldType>, TFieldType>> actions;

    public ForEachScope(
        IFieldDescriptor<TValidationType, IEnumerable<TFieldType>> fieldDescriptor,
        Action<IFieldDescriptor<IEnumerable<TFieldType>, TFieldType>> actions)
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
                var thingo = new FieldDescriptor<IEnumerable<TFieldType>, TFieldType>(
                    new IndexedPropertyExpressionToken<IEnumerable<TFieldType>, TFieldType>(
                        //fieldDescriptor.PropertyToken.Expression,
                        fieldDescriptor.PropertyToken.Name + $"[{index}]",
                        index
                        ),
                    //fieldDescriptor.ParentScope,
                    store//fieldDescriptor.Store,
                    );

                //thingo.IsAlwaysVital();

                store.AddItem(
                    thingo, new ForEachIndexedScope<TValidationType, TFieldType>(
                        store, thingo, actions)
                    );
                //actions?.Invoke(thingo);
                if (this.IsVital)
                {
                    var thingo2 = new FieldDescriptor<IEnumerable<TFieldType>, TFieldType>(
                        new IndexedPropertyExpressionToken<IEnumerable<TFieldType>, TFieldType>(
                            //fieldDescriptor.PropertyToken.Expression,
                            fieldDescriptor.PropertyToken.Name + $"[{index}]"+ Char.MaxValue,
                            index
                            ),
                        //fieldDescriptor.ParentScope,
                        store//fieldDescriptor.Store,
                        );
                    thingo2.NextValidationIsVital();
                    //thingo2.AddItem(this.IsVital, thingo, new VitallyForEachValidation(
                    //    fieldDescriptor.PropertyToken.Name + $"["
                    //    ));
                    thingo2.AddValidation(new VitallyForEachValidation(
                        fieldDescriptor.PropertyToken.Name + $"["
                        ));

                    //store.AddItem(
                    //thingo2, new ForEachIndexedScope<TValidationType, TFieldType>(
                    //    store, thingo2, () => { }
                    //);
                }

                index++;
            }
        }
    }

    protected override void InvokeScopeContainerToDescribe(ValidationConstructionStore store)
    {
        var thingo = new FieldDescriptor<IEnumerable<TFieldType>, TFieldType>(
            new IndexedPropertyExpressionToken<IEnumerable<TFieldType>, TFieldType>(
                //fieldDescriptor.PropertyToken.Expression,
                fieldDescriptor.PropertyToken.Name + $"[n]",
                -1
                ),
            store
            );

        store.AddItem(
            thingo, new ForEachIndexedScope<IEnumerable<TFieldType>, TFieldType>(
                store, thingo, actions)
            );
    }
}
