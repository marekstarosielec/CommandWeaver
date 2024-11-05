namespace Models;

public record Variable
{
    public required string Key { get; init; }

    public VariableValue Value { get; set; } = new VariableValue();
    
    public string? Description { get; init; }

    public List<string> AllowedValues { get; init; } = [];

    public VariableScope Scope { get; init; } = VariableScope.Command;

    public string? LocationId { get; set; }

    public bool Required { get; set; }
}