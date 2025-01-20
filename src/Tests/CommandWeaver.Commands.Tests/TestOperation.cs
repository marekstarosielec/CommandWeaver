using System.Collections.Immutable;

internal record TestOperation(string Name) : Operation
{
    public override string Name { get; } = Name;

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = ImmutableDictionary<string, OperationParameter>.Empty;
    public override async Task Run(CancellationToken cancellationToken)
    {
        // Simulate operation execution
        await Task.CompletedTask;
    }
}