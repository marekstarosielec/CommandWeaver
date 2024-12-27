public record InputInformation
{
    public required string Message { get; init; }

    public bool Required { get; init; } = false;
    
    public string? PromptStyle { get; init; }
}