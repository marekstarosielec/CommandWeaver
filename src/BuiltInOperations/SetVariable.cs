 using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class SetVariable: Operation
{
    public override string Name => nameof(SetVariable);

    //Add test - every Variable should be public, initialized, have getter and setter.
    public OperationParameter Key { get; set; } = new OperationParameter { Key = "key", Description = "Key of variable to set value to." };
    public OperationParameter Value { get; set; } = new OperationParameter { Key = "value", Description = "Value which should be set." };
   // public OperationParameter Description { get; set; } = new OperationParameter { Key = "description", Description = "Optional description for variable." };
    public OperationParameter Scope { get; set; } = new OperationParameter { Key = "scope", Description = "Optional scope for variable." };

    public SetVariable()
    {
        //This should be prefileld in variable + test to detect changes.
        foreach (var item in Enum.GetNames<VariableScope>())
            Scope.AllowedValues.Add(item);
    }

    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        //Add variables validation before running.
        context.Services.Output.Trace($"Getting variable key from {nameof(Key)}.");
        var key = context.Variables.ReadVariableValue(Key.Value);
        if (key?.TextValue == null)
        {
            //Name of variable to update must be resolved to string.
            context.Terminate($"{Key.Value} does not contain valid variable name.");
            return Task.CompletedTask;
        }

        context.Services.Output.Trace($"Getting variable value from {nameof(Value)}.");
        var newValue = context.Variables.ReadVariableValue(Value.Value);
        if (newValue == null)
        {
            //New value is required.
            //TODO: No value means remove variable.
            context.Terminate($"Value is required.");
            return Task.CompletedTask;
        }

        //context.Services.Output.Trace($"Getting description value from {nameof(Description)}.");
        //var description = context.Variables.ReadVariableValue(Description.Value);

        context.Services.Output.Trace($"Getting scope value from {nameof(Scope)}.");
        var scope = context.Variables.ReadVariableValue(Scope.Value);
        var scopeEnum = VariableScope.Command;
        if (!string.IsNullOrWhiteSpace(scope?.TextValue) && !Enum.TryParse(scope.TextValue, false, out scopeEnum))
        {
            context.Terminate($"Scope {scope.TextValue} is not a valid value");
            return Task.CompletedTask;
        }  

        context.Services.Output.Trace($"Value evaluated to not null");
        context.Variables.WriteVariableValue(scopeEnum, key.TextValue, newValue);
      
        return Task.CompletedTask;
    }
}