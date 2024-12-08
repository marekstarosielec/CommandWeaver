/// <summary>
/// Structure for  holding 2 values, e.g. for AreEqual condition.
/// </summary>
public class DoubleValue
{
    /// <summary>
    /// First value.
    /// </summary>
    public DynamicValue Value1 { get; init; } = new();
    
    /// <summary>
    /// Second value.
    /// </summary>
    public DynamicValue Value2 { get; init; } = new();
}