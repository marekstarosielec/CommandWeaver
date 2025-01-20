using System.Collections.Immutable;

public record Output(IOutputService outputService) : Operation
{
    public override string Name => nameof(Output);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "value", new OperationParameter { Description = "Value to output", Validation = new Validation { Required = true }}},
        { "styling", new OperationParameter { Description = "Kind of styling to apply", Validation = new Validation { AllowedEnumValues = typeof(Styling)}, DefaultValue = "Default" }},
        { "logLevel", new OperationParameter { Description = "Logging level", Validation = new Validation { AllowedEnumValues = typeof(LogLevel) } }}
    }.ToImmutableDictionary();
  
    public override Task Run(CancellationToken cancellationToken)
    {
        outputService.Write(
            Parameters["value"].Value,
            Parameters["logLevel"].GetEnumValue<LogLevel>(),
            Parameters["styling"].GetEnumValue<Styling>() ?? Styling.Default
        );
        return Task.CompletedTask;
    }
}