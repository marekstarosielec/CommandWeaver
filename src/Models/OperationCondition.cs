namespace Models;

public record OperationCondition
{
    public DynamicValue IsNull { get; set; } = new DynamicValue();
}
