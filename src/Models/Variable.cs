namespace Models;

public record Variable
{
    public required string Key { get; init; }

    public DynamicValue Value { get; set; } = new DynamicValue();
    
    //public VariableScope Scope { get; init; } = VariableScope.Command;

    public string? LocationId { get; set; }

    
    
    
    //public string? Description { get; init; }

    //public List<string> AllowedValues { get; init; } = [];
    
    //public bool Required { get; set; }
}