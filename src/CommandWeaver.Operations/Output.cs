public class Output(IOutput output) : Operation
{
    public override string Name => nameof(Output);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        { "text", new OperationParameter { Description = "Text to output", RequiredText = true } },
        { "formatting", new OperationParameter { Description = "Optional formatting", AllowedValues = ["default", "raw"] } },
        { "logLevel", new OperationParameter { Description = "Logging level", AllowedValues = ["trace", "debug", "information", "warning", "error"] } }
    };
  
    public override Task Run(CancellationToken cancellationToken)
    {
        output.Test(Parameters["text"].Value);
        // switch (Parameters["logLevel"].Value.TextValue)
        // {
        //     case "trace":
        //         output.Trace(Parameters["text"].Value.TextValue!);
        //         break;
        //     case "debug":
        //         output.Debug(Parameters["text"].Value.TextValue!);
        //         break;
        //     case "warning":
        //         output.Warning(Parameters["text"].Value.TextValue!);
        //         break;
        //     case "error":
        //         output.Error(Parameters["text"].Value.TextValue!);
        //         break;
        //     default:
        //         output.Result(Parameters["text"].Value.TextValue!, Parameters["formatting"].Value.TextValue);
        //         break;
        // }

        return Task.CompletedTask;
    }
}