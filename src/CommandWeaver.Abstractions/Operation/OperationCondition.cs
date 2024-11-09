public record OperationCondition
{
    public DynamicValue? IsNull { get; set; }

    public DynamicValue? IsNotNull { get; set; }
}
