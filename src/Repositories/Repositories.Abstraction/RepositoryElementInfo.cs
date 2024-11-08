namespace Repositories.Abstraction;

public class RepositoryElementInfo
{
    public required string Id { get; init; }
    
    public string? Format { get; init; }
    
    public string? FriendlyName { get; init; }
    
    public Exception? Exception { get; init; } 
}