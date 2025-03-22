using System.Collections.Immutable;

public record Terminate : Operation
{
    public override string Name => nameof(Terminate);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "message", new OperationParameter { Description = "Message displayed before terminating" } },
        { "exitCode", new OperationParameter { Description = "Application exit code", Validation = new Validation { AllowedType = "number"} } },
    }.ToImmutableDictionary(); 

    public override Task Run(CancellationToken cancellationToken)
    {
        //TODO: Add Default value to parameter when it is implemented.
        var exitCode = Parameters["exitCode"]?.Value.NumericValue ?? 1;
        throw new CommandWeaverException(Parameters["message"].Value.TextValue ?? string.Empty, (int) exitCode);
    }
}
