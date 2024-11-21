public abstract record OperationAggregate : Operation
{
    public string SerializedOperations { get; set; } = string.Empty;
    public List<Operation> Operations { get; set; } = [];
}