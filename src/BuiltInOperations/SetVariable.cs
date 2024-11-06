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
        { "scope", new OperationParameter { Description = "Optional scope for variable.", AllowedEnumValues = typeof(VariableScope) } },
    };


    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        context.Variables.WriteVariableValue(Parameters["scope"].Value.GetEnumValue<VariableScope>() ?? VariableScope.Command, Parameters["key"].Value.TextValue!, Parameters["value"].Value);
        return Task.CompletedTask;
    }
}