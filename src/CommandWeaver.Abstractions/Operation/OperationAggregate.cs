public abstract class OperationAggregate : Operation
{
    public List<Operation> Operations { get; } = new ();
    public string SerializedOperations { get; set; } = string.Empty;
}