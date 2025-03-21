using System.Collections.Immutable;

public record Base64Decode(IVariableService variableService, IFlowService flowService) : Operation
{
    public override string Name => nameof(Base64Decode);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "value", new OperationParameter { Description = "Value to decrypt", Validation = new Validation { Required = true, AllowedType = "text" }}},
        {"saveTo", new OperationParameter { Description = "Name of variable where decrypted value will be saved", Validation = new Validation { Required = true, AllowedType = "text"}}}
    }.ToImmutableDictionary();

    public override Task Run(CancellationToken cancellationToken)
    {
        var base64Encoded = Parameters["value"].Value.TextValue!;
        var saveTo = Parameters["saveTo"].Value.TextValue!;

        if (Convert.TryFromBase64String(base64Encoded, new Span<byte>(new byte[base64Encoded.Length]), out int bytesWritten))
        {
            var data = Convert.FromBase64String(base64Encoded);
            var decodedString = System.Text.Encoding.UTF8.GetString(data);

            variableService.WriteVariableValue(VariableScope.Command, saveTo, new DynamicValue(decodedString));
        }
        else
            flowService.Terminate($"{base64Encoded} is not valid base64");
        return Task.CompletedTask;
    }
}