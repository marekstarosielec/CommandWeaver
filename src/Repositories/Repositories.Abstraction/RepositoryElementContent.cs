namespace Repositories.Abstraction;

public class RepositoryElementContent
{
    public required string Id { get; init; }
    
    public string? Content { get; init; }
    
    public Exception? Exception { get; init; } 
}