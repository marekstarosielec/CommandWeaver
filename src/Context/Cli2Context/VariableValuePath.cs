using System.Text.RegularExpressions;

namespace Cli2Context;

internal static class VariableValuePath
{
    /// <summary>
    /// Returns whole path embedded into varaiable tags.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetWholePathAsVariable(string path) => $"{{{{ {path} }}}}";

    /// <summary>
    /// Extracts the string between specified delimiters within a given input string.
    /// </summary>
    /// <param name="input">The input string from which to extract the variable.</param>
    /// <returns>The extracted string between the delimiters, or null if the delimiters are not found.</returns>
    public static string? ExtractVariableBetweenDelimiters(string path)
    {
        int closingIndex = path.IndexOf("}}");
        if (closingIndex == -1) return null;

        int openingIndex = path.LastIndexOf("{{", closingIndex);
        if (openingIndex == -1) return null;

        return path.Substring(openingIndex + 2, closingIndex - openingIndex - 2).Trim();
    }

    /// <summary>
    /// Checks if whole path is single variable path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public static bool WholePathIsSingleVariable(string path, string variableName) => path.StartsWith("{{") && path.EndsWith("}}") && path.Trim('{', '}', ' ').Equals(variableName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Replaces variable tag in path with value.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="variableName"></param>
    /// <param name="variableValue"></param>
    /// <returns></returns>
    public static string ReplaceVariableWithValue(string path, string variableName, string variableValue) => Regex.Replace(path, $@"\{{\{{\s*{Regex.Escape(variableName)}\s*\}}\}}", variableValue);

    /// <summary>
    /// Checks if path points to top level variable.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool PathIsTopLevel(string path) => path.IndexOfAny(['.', '[']) == -1;

    /// <summary>
    /// Returns collection of path sections.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static MatchCollection GetPathSections(string path) => Regex.Matches(path, @"([a-zA-Z0-9_\-\s]+)|\[(.*?)\]");
}
