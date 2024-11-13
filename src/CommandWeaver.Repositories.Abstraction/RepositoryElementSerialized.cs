/// <summary>
/// Contains information about repository element (e.g. file on disc).
/// </summary>
public record RepositoryElementSerialized
{
    /// <summary>
    /// Identifier, it should be unique among repository.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Content type, e.g. json.
    /// </summary>
    public string? Format { get; init; }
    
    /// <summary>
    /// Repository name dispolayed to user.
    /// </summary>
    public string? FriendlyName { get; init; }
    
    /// <summary>
    /// Repository content in serialized form.
    /// </summary>
    public string? Content { get; init; }
}