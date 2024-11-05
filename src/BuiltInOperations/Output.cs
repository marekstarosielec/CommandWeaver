using Models;
using Models.Interfaces.Context;

// ReSharper disable once CheckNamespace

namespace BuiltInOperations;

public class Output : Operation
{
    public override string Name => nameof(Output);

    public Variable Text { get; set; } = new Variable { Key = "text",  Description = "Text to output"};
    
    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        var text = context.Variables.ReadVariableValue(Text.Value);
        if (!string.IsNullOrWhiteSpace(text?.TextValue))
            context.Services.Output.Warning(text.TextValue);
        return Task.CompletedTask;
    }
}