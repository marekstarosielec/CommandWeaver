namespace CommandWeaver.Operations;

public record ForEach(ICommands commands, IVariables variables, ISerializerFactory serializerFactory) : OperationAggregate
{
    public override string Name => nameof(ForEach);

    public override Dictionary<string, OperationParameter> Parameters { get; } =
        new Dictionary<string, OperationParameter>
        {
            {"list", new OperationParameter { Description = "List to enumerate through", RequiredList = true}},
            {"element", new OperationParameter { Description = "Name of variable where each element of list will be placed", RequiredText = true}}
        };

    public override async Task Run(CancellationToken cancellationToken)
    {
        var path = Parameters["element"].Value.TextValue!;
        foreach (var element in Parameters["list"].Value.ListValue!)
        {
            variables.WriteVariableValue(VariableScope.Command, path, new DynamicValue(element));
            await commands.ExecuteOperations(GetOperationsCopy(), cancellationToken);
        }
    }

    private List<Operation> GetOperationsCopy()
    {
        var serializer = serializerFactory.GetDefaultSerializer(out _);
        serializer.TryDeserialize(SerializedOperations, out List<Operation> operations, out _);
        return operations;
    }
}