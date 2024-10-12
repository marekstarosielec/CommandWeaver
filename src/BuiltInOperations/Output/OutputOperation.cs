using Models;
using Models.Interfaces.Context;

// ReSharper disable once CheckNamespace

namespace BuiltInOperations;

public class OutputOperation : Operation
{
    public Variable Text { get; set; } = new Variable { Key = "text",  Description = "Text to output"};
    
    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        var text = context.Variables.GetVariableValue2(Text.Value as string) as string;
        if (!string.IsNullOrWhiteSpace(text))
            context.Services.Output.Warning(text);
        return Task.CompletedTask;
    }
}