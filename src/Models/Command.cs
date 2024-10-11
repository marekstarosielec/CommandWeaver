namespace Models;

public class Command
{
    public required string Name { get; init; }
    
    // public string? ShortName { get; init; }
    //
    public List<Operation> Operations { get; set; } = [];

    public List<Variable> Parameters { get; set; } = [];
}