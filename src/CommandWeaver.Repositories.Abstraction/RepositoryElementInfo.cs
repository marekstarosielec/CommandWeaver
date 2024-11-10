public record RepositoryElementInfo
{
    public required string Id { get; init; }
    
    public string? Format { get; init; }
    
    public string? FriendlyName { get; init; }
    
    public string? Content { get; init; }

    public Exception? Exception { get; init; } 
}