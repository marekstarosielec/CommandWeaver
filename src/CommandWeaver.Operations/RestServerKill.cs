using System.Collections.Immutable;
// ReSharper disable ClassNeverInstantiated.Global

public record RestServerKill(IBackgroundService backgroundService) : Operation
{
    public override string Name => nameof(RestServerKill);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            // { 
            //     //TODO: When it will be possible to run multiple rest servers, this property will be required to know which should be killed.
            //     "port",
            //     new OperationParameter
            //     {
            //         Description = "Port where calls are received",
            //         Validation = new Validation { Required = true, AllowedType = "number" }
            //     }
            // }
        }.ToImmutableDictionary();
    
    public override Task Run(CancellationToken cancellationToken)
    {
        backgroundService.Stop();
        return Task.CompletedTask;
    }
}

