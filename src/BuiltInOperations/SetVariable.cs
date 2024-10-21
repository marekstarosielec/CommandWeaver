using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class SetVariable: Operation
{
    public override string Name => nameof(SetVariable);

    //Add test - every Variable should be public, initialized, have getter and setter.
    public Variable Key { get; set; } = new Variable { Key = "key", Description = "Key of variable to set value to." };
    public Variable Value { get; set; } = new Variable { Key = "value", Description = "Value which should be set." };
    public Variable Description { get; set; } = new Variable { Key = "description", Description = "Optional description for variable." };
    public Variable Scope { get; set; } = new Variable { Key = "scope", Description = "Optional scope for variable." };

    public SetVariable()
    {
        foreach (var item in Enum.GetNames<VariableScope>())
            Scope.AllowedValues.Add(item);
    }

    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        //Add variables validation before running.
        context.Services.Output.Trace($"Getting variable key from {nameof(Key)} as string");
        var key = context.Variables.GetValueAsString(Key.Value);
        if (key == null)
        {
            context.Services.Output.Error($"{Key.Value} evaluated to null.");
            return Task.CompletedTask;
        }

        object? newValue = null;
        context.Services.Output.Trace($"Getting variable value from {nameof(Value)} as string");
        var newValueString = context.Variables.GetValueAsString(Value.Value);
        if (newValueString != null)
        {
            context.Services.Output.Trace($"Value evaluated to not null");
            newValue = newValueString;
        }
        if (newValue == null)
        {
            context.Services.Output.Trace($"Getting variable value from {nameof(Value)} as object");
            var newValueObject = context.Variables.GetValueAsObject(Value.Value);
            if (newValueObject != null)
            {
                context.Services.Output.Trace($"Value evaluated to not null");
                newValue = newValueObject;
            }
        }
        if (newValue == null)
        {
            context.Services.Output.Trace($"Getting variable value from {nameof(Value)} as list");
            var newValueList = context.Variables.GetValueAsList(Value.Value);
            if (newValueList != null)
            {
                context.Services.Output.Trace($"Value evaluated to not null");
                newValue = newValueList;
            }
        }

        context.Services.Output.Trace($"Getting variable description from {nameof(Description)} as string");
        var description = context.Variables.GetValueAsString(Description.Value);

        context.Services.Output.Trace($"Getting variable scope from {nameof(Scope)} as string string");
        var scope = context.Variables.GetValueAsString(Scope.Value);
        var scopeEnum = VariableScope.Command;
        if (!string.IsNullOrWhiteSpace(scope))
            Enum.TryParse(scope, true, out scopeEnum);

        context.Services.Output.Trace($"Setting variable value");
        context.Variables.SetVariableValue(scopeEnum, key, newValue, description);
        return Task.CompletedTask;
    }
}