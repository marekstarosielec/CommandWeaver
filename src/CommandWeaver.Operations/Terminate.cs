using System.Collections.Immutable;

public record Terminate(IFlowService flowService) : Operation
{
    public override string Name => nameof(Terminate);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "message", new OperationParameter { Description = "Message displayed before terminating" } },
        //TODO: Add exit code - need to check numeric DynamicValue handling.
    }.ToImmutableDictionary(); 

    public override Task Run(CancellationToken cancellationToken)
    {
        flowService.Terminate(Parameters["message"].Value.TextValue);
        return Task.CompletedTask;
    }
}
