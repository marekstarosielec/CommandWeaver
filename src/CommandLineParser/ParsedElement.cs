/// <summary>
/// Internal structure for gathering results of command line parsing.
/// </summary>
/// <param name="name">Name of the parsed element.</param>
/// <param name="value">Nullable value (only for arguments).</param>
/// <param name="type">Type of the element (command or argument).</param>
internal readonly struct ParsedElement(string name, string? value, string type)
{
    /// <summary>
    /// Name of the parsed element.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Nullable value (only for arguments).
    /// </summary>
    public string? Value { get; } = value;

    /// <summary>
    /// Type of the element (command or argument).
    /// </summary>
    public string Type { get; } = type;   
}
