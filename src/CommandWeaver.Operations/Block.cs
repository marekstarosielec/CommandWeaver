using System.Collections.Immutable;

public record Block(ICommandService commandService) : OperationAggregate
{
    public override string Name => nameof(Block);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>().ToImmutableDictionary();

    public override Task Run(CancellationToken cancellationToken) => commandService.ExecuteOperations(Operations, cancellationToken);
}