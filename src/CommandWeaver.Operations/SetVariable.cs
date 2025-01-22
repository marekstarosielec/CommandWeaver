using System.Collections.Immutable;

public record SetVariable(IVariableService variableService) : Operation
{
    public override string Name => nameof(SetVariable);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "key", new OperationParameter { Description = "Key of variable to set value to.", Validation = new Validation { Required = true, AllowedType = "text" }}},
        { "value", new OperationParameter { Description = "Value which should be set." }},
        { "scope", new OperationParameter { Description = "Optional scope for variable.", Validation = new Validation { AllowedEnumValues = typeof(VariableScope) }}},
        { "id", new OperationParameter { Description = "Optional name of file where variable should be stored." } },
    }.ToImmutableDictionary();

    public override Task Run(CancellationToken cancellationToken)
    {
        variableService.WriteVariableValue(
            Parameters["scope"].GetEnumValue<VariableScope>() ?? VariableScope.Command, 
            Parameters["key"].Value.TextValue!, 
            Parameters["value"].Value,
            Parameters["id"].Value.TextValue);
        return Task.CompletedTask;
    }
}