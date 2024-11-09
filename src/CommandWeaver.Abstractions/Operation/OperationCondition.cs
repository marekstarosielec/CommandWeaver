/// <summary>
/// Represents conditions for executing an operation,.
/// </summary>
public record OperationCondition
{
    /// <summary>
    /// Gets or sets the condition specifying that the operation should execute if a value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// When this condition is set, the operation will execute only if the associated value is <c>null</c>.
    /// </remarks>
    public DynamicValue? IsNull { get; set; }

    /// <summary>
    /// Gets or sets the condition specifying that the operation should execute if a value is not <c>null</c>.
    /// </summary>
    /// <remarks>
    /// When this condition is set, the operation will execute only if the associated value is not <c>null</c>.
    /// </remarks>
    public DynamicValue? IsNotNull { get; set; }
}