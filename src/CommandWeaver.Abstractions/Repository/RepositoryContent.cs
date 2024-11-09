/// <summary>
/// Represents the content of a repository, including commands and variables.
/// </summary>
public record RepositoryContent
{
    /// <summary>
    /// Gets or sets the list of commands stored in the repository.
    /// </summary>
    /// <remarks>
    /// Each command defines a set of operations and parameters. This list may contain <c>null</c> entries.
    /// </remarks>
    public List<Command>? Commands { get; set; }

    /// <summary>
    /// Gets or sets the list of variables stored in the repository.
    /// </summary>
    /// <remarks>
    /// Each variable represents a configurable or dynamic value associated with the repository. This list may contain <c>null</c> entries.
    /// </remarks>
    public List<Variable>? Variables { get; set; }
}