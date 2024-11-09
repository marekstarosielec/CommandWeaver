namespace BuiltInOperations;

public class Output : Operation
{
    public override string Name => nameof(Output);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "text", new OperationParameter { Description = "Text to output.", RequiredText = true } }
    };
  
    public override Task Run(IContext context, CancellationToken cancellationToken)
    {
        context.Services.Output.Error(Parameters["text"].Value.TextValue!);
        return Task.CompletedTask;
    }
}