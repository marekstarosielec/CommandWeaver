using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class SetVariable: Operation
{
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

        var key = context.Variables.GetValueAsString(Key.Value);
        var currentValue = context.Variables.GetValueAsString(key, true);
        var newValue = context.Variables.GetValueAsString(Value.Value);
        var description = context.Variables.GetValueAsString(Description.Value);
        var scope = context.Variables.GetValueAsString(Scope.Value);
        var scopeEnum = VariableScope.Command;
        if (!string.IsNullOrWhiteSpace(scope))
            Enum.TryParse(scope, true, out scopeEnum);

        context.Variables.SetVariableValue(scopeEnum, key, newValue, description);
        return Task.CompletedTask;
    }
}