/// <summary>
/// Represents a variable with a key, value, and optional location identifier within a repository.
/// </summary>
public record Variable
{
    /// <summary>
    /// Gets the unique key that identifies this variable.
    /// </summary>
    /// <remarks>This is a required field and serves as the identifier for the variable.</remarks>
    public required string Key { get; init; }

    /// <summary>
    /// Gets or sets the value associated with this variable.
    /// </summary>
    /// <remarks>This value can be dynamically assigned and modified.</remarks>
    public DynamicValue Value { get; set; } = new DynamicValue();

    /// <summary>
    /// Gets or sets an optional repository element identifier for this variable.
    /// It will allow to save changes back to same location.
    /// </summary>
    /// <remarks>
    /// This ID indicates the specific location context for the variable, allowing it to be scoped 
    /// or associated with a particular repository location.
    /// </remarks>
    public string? RepositoryElementId { get; set; }
}