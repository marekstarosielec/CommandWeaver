namespace Models;

public record CommandParameter
{
    public required string Key { get; init; }
    public string Description { get; set; }
    public bool Required { get; set; }
}
