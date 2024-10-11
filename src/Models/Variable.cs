using System.Text.RegularExpressions;
using Models.Interfaces.Context;

namespace Models;

public record Variable
{
    public required string Key { get; init; }

    public object? Value { get; set; }
    
    public string? Description { get; init; }
    
    public VariableScope Scope { get; init; } = VariableScope.Command;

    public string? GetValueAsString(IContext context)
    {
        return EvaluateVariables(context, Value as string);
    }

    private string? EvaluateVariables(IContext context, string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;
        
        var pattern = @"\{\{\s*(.*?)\s*\}\}"; // Regex pattern to match {{ content }}
        var regex = new Regex(pattern);

        //TODO: this might need matching inner variables (variables inside variables) first.
        while (regex.IsMatch(input))
        {
            input = regex.Replace(input, match =>
            {
                // Extract the content between {{ and }}
                var key = match.Groups[1].Value;

                // Get the replacement from the provided method
                var replacement = context.Variables.GetVariableValue2(key) as string ?? string.Empty;

                return replacement;
            });
        }

        return input;
    }
}