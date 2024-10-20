using Models.Interfaces;
using System.Text.RegularExpressions;

namespace Cli2Context;

/// <inheritdoc />
internal class VariableExtractionService : IVariableExtractionService
{
    /// <inheritdoc />
    public string? ExtractVariableBetweenDelimiters(string input)
    {
        // Find the position of the closing braces }}
        int closingIndex = input.IndexOf("}}");
        if (closingIndex == -1) return null;

        // Find the position of the opening braces {{ before the closing braces
        int openingIndex = input.LastIndexOf("{{", closingIndex);
        if (openingIndex == -1) return null;

        // Extract the string between {{ and }}
        return input.Substring(openingIndex + 2, closingIndex - openingIndex - 2).Trim();
    }

    /// <inheritdoc />
    public string ReplaceVariableInString(string input, string variableName, string? variableValue)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(variableName))
            return input;

        // Pattern to match {{ variableName }} with possible whitespaces
        string pattern = $@"\{{\{{\s*{Regex.Escape(variableName)}\s*\}}\}}";

        // Replace all occurrences with the variable value
        return Regex.Replace(input, pattern, variableValue ?? string.Empty);
    }
}
