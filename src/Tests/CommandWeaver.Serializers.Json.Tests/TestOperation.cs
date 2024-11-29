// Test-specific implementations of Operation and OperationAggregate

using System.Collections.Immutable;

internal record TestOperation : Operation
{
    public override string Name => "TestOperation";
    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        ImmutableDictionary<string, OperationParameter>.Empty;
    
    public override Task Run(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal record TestAggregateOperation : OperationAggregate
{
    public override string Name => "TestAggregateOperation";
    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        ImmutableDictionary<string, OperationParameter>.Empty;

    public override Task Run(CancellationToken cancellationToken) => Task.CompletedTask;
}