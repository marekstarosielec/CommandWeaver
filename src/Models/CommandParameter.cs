namespace Models;

public record CommandParameter
{
    public required string Key { get; init; }
    public string? Description { get; set; }
    public List<string> AllowedValues { get; init; } = [];
    
    public bool Required { get; set; }
}
