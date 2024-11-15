/// <summary>
/// Represents a parameter for a command.
/// </summary>
public record CommandParameter
{
    /// <summary>
    /// Gets or sets the key for the command parameter.
    /// </summary>
    /// <remarks>This field is required and serves as the unique identifier for the parameter.</remarks>
    public required string Key { get; init; }

    public virtual List<string>? OtherNames { get; init; }

    /// <summary>
    /// Gets or sets a description of the command parameter.
    /// </summary>
    /// <remarks>This provides additional information or guidance on the parameter's usage.</remarks>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the list of allowed values for the command parameter.
    /// </summary>
    /// <remarks>If specified, only these values are accepted for the parameter.</remarks>
    public List<string>? AllowedValues { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this command parameter is required.
    /// </summary>
    /// <remarks>If <c>true</c>, the parameter must be provided; otherwise, it is optional.</remarks>
    public bool Required { get; set; }
    
    public Type? AllowedEnumValues { get; set; }
}