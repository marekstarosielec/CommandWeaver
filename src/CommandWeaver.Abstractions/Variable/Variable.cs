public record Variable
{
    public required string Key { get; init; }

    public DynamicValue Value { get; set; } = new DynamicValue();
    
    public string? LocationId { get; set; }
}