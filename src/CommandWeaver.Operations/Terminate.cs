using System.Collections.Immutable;

public record Terminate(IFlowService flow) : Operation
{
    public override string Name => nameof(Terminate);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "message", new OperationParameter { Description = "Message displayed before terminating" } },
        //TODO: Add exit code.
    }.ToImmutableDictionary(); 

    public override Task Run(CancellationToken cancellationToken)
    {
        flow.Terminate(Parameters["message"].Value.TextValue);
        return Task.CompletedTask;
    }
}
