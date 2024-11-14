namespace BuiltInOperations;

public class Output(IOutput output) : Operation
{
    public override string Name => nameof(Output);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "text", new OperationParameter { Description = "Text to output", RequiredText = true } },
        { "formatting", new OperationParameter { Description = "Optional formatting", AllowedValues = ["default", "raw"] } }
    };
  
    public override Task Run(CancellationToken cancellationToken)
    {
        output.Result(Parameters["text"].Value.TextValue!, Parameters["formatting"].Value.TextValue);
        return Task.CompletedTask;
    }
}