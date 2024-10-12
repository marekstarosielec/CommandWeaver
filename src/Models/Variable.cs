using System.Text.RegularExpressions;
using Models.Interfaces.Context;

namespace Models;

public record Variable
{
    public required string Key { get; init; }

    public object? Value { get; set; }
    
    public string? Description { get; init; }
    
    public VariableScope Scope { get; init; } = VariableScope.Command;
}