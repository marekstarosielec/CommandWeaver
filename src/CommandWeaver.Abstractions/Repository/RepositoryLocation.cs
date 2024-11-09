/// <summary>
/// Specifies the location type for a repository, indicating the scope and mutability of stored data.
/// </summary>
public enum RepositoryLocation
{
    /// <summary>
    /// Represents a built-in repository location.
    /// </summary>
    /// <remarks>This location is internal, and data is unchangeable locally.</remarks>
    BuiltIn = 0,

    /// <summary>
    /// Represents an application-wide repository location.
    /// </summary>
    /// <remarks>Data stored here is accessible across all sessions.</remarks>
    Application = 1,

    /// <summary>
    /// Represents a session-specific repository location.
    /// </summary>
    /// <remarks>Data stored here is unique to the current session.</remarks>
    Session = 2
}