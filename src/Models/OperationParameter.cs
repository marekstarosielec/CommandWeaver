namespace Models;

public record OperationParameter
{

    public DynamicValue Value { get; set; } = new DynamicValue();

    public string? Description { get; init; }

    public List<string> AllowedValues { get; init; } = [];

    public bool Required { get; set; }
}
