using System.Collections.Immutable;

public record ForEach(ICommandService commandService, IVariableService variableService, IOutputService outputService) : OperationAggregate
{
    public override string Name => nameof(ForEach);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            {"list", new OperationParameter { Description = "List to enumerate through"}},
            {"element", new OperationParameter { Description = "Name of variable where each element of list will be placed", Validation = new Validation { Required = true, AllowedType = "text"}}}
        }.ToImmutableDictionary();

    public override async Task Run(CancellationToken cancellationToken)
    {
        var path = Parameters["element"].Value.TextValue!;
        var elements = Parameters["list"].Value.ListValue ??
                       new DynamicValueList([Parameters["list"].Value]) ;
        foreach (var element in elements)
        {
            outputService.Debug("Processing element in list");
            variableService.WriteVariableValue(VariableScope.Command, path, element);
            await commandService.ExecuteOperations(Operations, cancellationToken);
        }
    }
}