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
        context.Services.Output.Trace($"Getting variable key from {nameof(Key)}.");
        var key = context.Variables.ResolveVariableValue(Key.Value);
        if (key?.TextValue == null)
        {
            //Name of variable to update must be resolved to string.
            context.Services.Output.Error($"{Key.Value} does not contain valid variable name.");
            return Task.CompletedTask;
        }

        context.Services.Output.Trace($"Getting variable value from {nameof(Value)}.");
        var newValue = context.Variables.ResolveVariableValue(Value.Value);
        if (newValue == null)
        {
            //New value is required.
            //TODO: No value means remove variable.
            context.Services.Output.Error($"Value is required.");
            Environment.Exit(1);
            return Task.CompletedTask;
        }

        context.Services.Output.Trace($"Getting description value from {nameof(Description)}.");
        var description = context.Variables.ResolveVariableValue(Description.Value);

        context.Services.Output.Trace($"Getting scope value from {nameof(Scope)}.");
        var scope = context.Variables.ResolveVariableValue(Scope.Value);

        context.Services.Output.Trace($"Value evaluated to not null");
            context.Variables.SetVariableValue(VariableScope.Command, key.TextValue, newValue);
        
        //if (newValue == null)
        //{
        //    context.Services.Output.Trace($"Getting variable value from {nameof(Value)} as object");
        //    var newValueObject = context.Variables.GetValueAsObject(Value.Value);
        //    if (newValueObject != null)
        //    {
        //        context.Services.Output.Trace($"Value evaluated to not null");
        //        newValue = newValueObject;
        //    }
        //}
        //if (newValue == null)
        //{
        //    context.Services.Output.Trace($"Getting variable value from {nameof(Value)} as list");
        //    var newValueList = context.Variables.GetValueAsList(Value.Value);
        //    if (newValueList != null)
        //    {
        //        context.Services.Output.Trace($"Value evaluated to not null");
        //        newValue = newValueList;
        //    }
        //}

        //context.Services.Output.Trace($"Getting variable description from {nameof(Description)} as string");
        //var description = context.Variables.GetValueAsString(Description.Value);

        //context.Services.Output.Trace($"Getting variable scope from {nameof(Scope)} as string string");
        //var scope = context.Variables.GetValueAsString(Scope.Value);
        //var scopeEnum = VariableScope.Command;
        //if (!string.IsNullOrWhiteSpace(scope))
        //    Enum.TryParse(scope, true, out scopeEnum);

        context.Services.Output.Trace($"Setting variable value");
       // context.Variables.SetVariableValue(scopeEnum, key, newValue, description);
        return Task.CompletedTask;
    }
}