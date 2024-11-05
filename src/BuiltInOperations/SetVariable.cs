 using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class SetVariable : Operation
{
    public override string Name => nameof(SetVariable);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "key", new OperationParameter { Description = "Key of variable to set value to.", RequiredText = true } },
        { "value", new OperationParameter { Description = "Value which should be set.", Required = true } },
        { "scope", new OperationParameter { Description = "Optional scope for variable." } },
    };

    public SetVariable()
    {
        //This should be prefileld in variable + test to detect changes (?).
        foreach (var item in Enum.GetNames<VariableScope>())
            Parameters["scope"].AllowedValues.Add(item);
    }

    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        var scopeEnum = VariableScope.Command;
        if (!string.IsNullOrWhiteSpace(Parameters["scope"].Value?.TextValue) && !Enum.TryParse(Parameters["scope"].Value.TextValue, false, out scopeEnum))
        {
            context.Terminate($"Scope {Parameters["scope"].Value?.TextValue} is not a valid value");
            return Task.CompletedTask;
        }  

        context.Services.Output.Trace($"Value evaluated to not null");
        context.Variables.WriteVariableValue(scopeEnum, Parameters["key"].Value.TextValue!, Parameters["value"].Value);
      
        return Task.CompletedTask;
    }
}