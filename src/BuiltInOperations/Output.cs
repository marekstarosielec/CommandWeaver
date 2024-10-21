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
        var text = context.Variables.GetValueAsString(Text.Value, true);
        if (!string.IsNullOrWhiteSpace(text))
            context.Services.Output.Warning(text);
        return Task.CompletedTask;
    }
}