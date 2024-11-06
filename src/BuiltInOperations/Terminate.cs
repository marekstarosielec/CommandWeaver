using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class Terminate : Operation
{
    public override string Name => nameof(Terminate);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "message", new OperationParameter { Description = "Message displayed before terminating" } },
        //TODO: Add exit code.
    }; 

    public override async Task Run(IContext context, CancellationToken cancellationToken)
    {
        await Task.Delay(10000, cancellationToken);
        context.Terminate(Parameters["message"].Value.TextValue);
        //return Task.CompletedTask;
    }
}
