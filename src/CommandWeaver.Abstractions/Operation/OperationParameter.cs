/// <summary>
/// Represents a parameter used in an operation.
/// </summary>
public record OperationParameter
{
    /// <summary>
    /// Gets or sets the dynamic value for the operation parameter.
    /// </summary>
    /// <remarks>This value holds the current input or setting for the parameter.</remarks>
    public DynamicValue Value { get; set; } = new DynamicValue();

    /// <summary>
    /// Gets an optional description of the operation parameter.
    /// </summary>
    /// <remarks>Provides additional context or usage information for the parameter.</remarks>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the list of allowed values for this parameter.
    /// </summary>
    /// <remarks>If specified, only values within this list are valid for the parameter.</remarks>
    public List<string>? AllowedValues { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this parameter is required.
    /// </summary>
    /// <remarks>If <c>true</c>, the parameter must be provided for the operation to execute.</remarks>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether text value is required for this parameter.
    /// </summary>
    /// <remarks>If <c>true</c>, the parameter expects a text value as its value.</remarks>
    public bool RequiredText { get; set; }

    /// <summary>
    /// Gets or sets the allowed enum type for this parameter.
    /// </summary>
    /// <remarks>
    /// If specified, the parameter value is restricted to valid values of this enum type.
    /// This allows for type-safe, predefined options.
    /// </remarks>
    public Type? AllowedEnumValues { get; set; }
}
