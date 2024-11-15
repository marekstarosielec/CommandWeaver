using System.Text.RegularExpressions;

/// <summary>
/// Helper class containing methods that can analyze variable path syntax.
/// </summary>
internal static class ValuePath
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
    public static bool WholePathIsSingleVariable(string path, string variableName) 
    {
        return path.StartsWith("{{") && path.EndsWith("}}") && path.Trim('{', '}', ' ').Equals(variableName) && path.Trim().IndexOf("{{", 2, StringComparison.Ordinal) == -1;
        // if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(variableName))
        //     return false;
        //
        // // Remove all [[...]] patterns including their content
        // path = Regex.Replace(path, @"\[\[.*?\]\]", "").Trim();
        //
        // // Check if path matches {{variableName}}
        // return path == $"{{{{{variableName}}}}}";
    }
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
    /// Checks if path points to top level list.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool PathIsTopLevelList(string path) => path.IndexOf('.') == -1 && path.IndexOf('[') > -1;

    /// <summary>
    /// Returns key in top level list path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string? TopLevelListKey(string path) => PathIsTopLevelList(path)
        ? path.Substring(path.IndexOf('[') + 1, path.IndexOf(']') - path.IndexOf('[') - 1)
        : null;

    /// <summary>
    /// Returns collection of path sections.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static MatchCollection GetPathSections(string path) => Regex.Matches(path, @"([a-zA-Z0-9_\-\s]+)|\[(.*?)\]");

    /// <summary>
    /// Returns first section of part.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetVariableName(string path) => path.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries).First();
}
