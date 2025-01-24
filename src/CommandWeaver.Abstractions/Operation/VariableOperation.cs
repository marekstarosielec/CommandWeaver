using System.Collections.Immutable;

public record VariableOperation : Operation
{
    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>().ToImmutableDictionary();

    public override string Name { get; } = nameof(VariableOperation);
    
    public override Task Run(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}