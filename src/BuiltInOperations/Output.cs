using Models;
using Models.Interfaces.Context;

namespace BuiltInOperations;

public class Output : Operation
{
    public override string Name => nameof(Output);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "text", new OperationParameter { Description = "Text to output." } }
    };
  
    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        var text = context.Variables.ReadVariableValue(Parameters["text"].Value);
        if (!string.IsNullOrWhiteSpace(text?.TextValue))
            context.Services.Output.Warning(text.TextValue);
        return Task.CompletedTask;
    }
}