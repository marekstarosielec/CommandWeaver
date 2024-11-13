namespace BuiltInOperations;

public class SetVariable(IVariables variables) : Operation
{
    public override string Name => nameof(SetVariable);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "key", new OperationParameter { Description = "Key of variable to set value to.", RequiredText = true } },
        { "value", new OperationParameter { Description = "Value which should be set.", Required = true } },
        { "scope", new OperationParameter { Description = "Optional scope for variable.", AllowedEnumValues = typeof(VariableScope) } },
        { "id", new OperationParameter { Description = "Optional name of file where variable should be stored." } },
    };


    public override Task Run(CancellationToken cancellationToken)
    {
        variables.WriteVariableValue(
            Parameters["scope"].Value.GetEnumValue<VariableScope>() ?? VariableScope.Command, 
            Parameters["key"].Value.TextValue!, 
            Parameters["value"].Value,
            Parameters["id"].Value.TextValue);
        return Task.CompletedTask;
    }
}