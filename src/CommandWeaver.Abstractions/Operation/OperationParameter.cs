using System.Collections.Immutable;

/// <summary>
/// Represents a parameter used in an operation.
/// </summary>
public record OperationParameter
{
    /// <summary>
    /// Gets or sets the dynamic value for the operation parameter.
    /// </summary>
    /// <remarks>
    /// This value holds the current input or resolved setting for the parameter, typically after processing 
    /// any dynamic placeholders or metatags.
    /// </remarks>
    public DynamicValue Value { get; set; } = new DynamicValue();

    /// <summary>
    /// Gets or sets the original dynamic value for the operation parameter.
    /// </summary>
    /// <remarks>
    /// This value stores the initial input or unresolved state of the parameter, including any placeholders or metatags
    /// (e.g., <c>{{variable}}</c>) before resolution. It is useful for debugging or exporting the raw input state.
    /// </remarks>
    public DynamicValue OriginalValue { get; init; } = new ();

    /// <summary>
    /// Gets a description of the operation parameter.
    /// </summary>
    /// <remarks>
    /// Provides additional context or guidance on the parameter's purpose and usage.
    /// </remarks>
    public required string Description { get; init; }

    /// <summary>
    /// Value validation information.
    /// </summary>
    public Validation? Validation { get; set; }
    
    /// <summary>
    /// Gets or sets the default value for this parameter.
    /// </summary>
    /// <remarks>
    /// If the parameter value is not provided, this default value will be used.
    /// </remarks>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Attempts to retrieve an enum value from the stored text value if it matches the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type to attempt parsing.</typeparam>
    /// <returns>
    /// The parsed enum value if successful, or the default enum value if the current value is invalid
    /// and a default value is provided. Otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method parses the current value or the default value into the specified enum type. If neither 
    /// can be parsed, the method returns <c>null</c>.
    /// </remarks>
    public T? GetEnumValue<T>() where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(Value.TextValue) && string.IsNullOrWhiteSpace(DefaultValue))
            return null;

        if (Enum.TryParse(Value.TextValue, true, out T result))
            return result;

        if (Enum.TryParse(DefaultValue, true, out T defaultResult))
            return defaultResult;

        return null;
    }
}
