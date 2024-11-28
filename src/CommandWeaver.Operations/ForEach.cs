using System.Collections.Immutable;

public record ForEach(ICommandService commandService, IVariableService variableService) : OperationAggregate
{
    public override string Name => nameof(ForEach);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            {"list", new OperationParameter { Description = "List to enumerate through", RequiredList = true}},
            {"element", new OperationParameter { Description = "Name of variable where each element of list will be placed", RequiredText = true}}
        }.ToImmutableDictionary();

    public override async Task Run(CancellationToken cancellationToken)
    {
        var path = Parameters["element"].Value.TextValue!;
        foreach (var element in Parameters["list"].Value.ListValue!)
        {
            variableService.WriteVariableValue(VariableScope.Command, path, new DynamicValue(element.ObjectValue));
            await commandService.ExecuteOperations(Operations.ToList(), cancellationToken);
        }
    }
}