/// <summary>
/// Represents conditions for executing an operation.
/// </summary>
public record Condition
{
    /// <summary>
    /// Gets or sets the condition specifying that the operation should execute if a value is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// When this condition is set, the operation will execute only if the associated value is <c>null</c>.
    /// </remarks>
    public DynamicValue? IsNull { get; init; }

    /// <summary>
    /// Gets or sets the condition specifying that the operation should execute if a value is not <c>null</c>.
    /// </summary>
    /// <remarks>
    /// When this condition is set, the operation will execute only if the associated value is not <c>null</c>.
    /// </remarks>
    public DynamicValue? IsNotNull { get; init; }
    
    /// <summary>
    /// Gets or sets the condition specifying that the operation should execute if both values are equal.
    /// </summary>
    public DoubleValue? AreEqual { get; init; }
    
    /// <summary>
    /// Gets or sets the condition specifying that the operation should execute if both values are not equal.
    /// </summary>
    public DoubleValue? AreNotEqual { get; init; }
}