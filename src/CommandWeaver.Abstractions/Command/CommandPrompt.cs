public record CommandPrompt
{
    public bool Enabled { get; init; } = true;
    
    public string? Message { get; init; }
    
    public string? PromptStyle { get; init; }
}