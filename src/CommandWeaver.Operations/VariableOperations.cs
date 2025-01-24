using System.Collections.Immutable;

/// <summary>
/// Operation used when variable name is provided as operation. Cannot be instantiated by name.
/// </summary>
/// <param name="commandService"></param>
public record VariableOperations(ICommandService commandService, IOperationFactory operationFactory, IVariableService variableService, IResourceService ResourceService) : OperationAggregate
{
    public override string Name => nameof(VariableOperations);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            { "variable", new OperationParameter { Description = "Name of variable that contains operation(s)"}}
        }.ToImmutableDictionary();

    public override Task Run(CancellationToken cancellationToken)
    {
        var t = Parameters["variable"].Value.ObjectValue?["operation"]?.TextValue;
        if (t == null)
            return Task.CompletedTask;
        var t1 = operationFactory.GetOperation(t);

        var t5 = Parameters["variable"].OriginalValue.TextValue;
        //variableService.ReadVariableValue(t)

        return commandService.ExecuteOperations(Operations, cancellationToken);
    }
}