using System.Collections.Immutable;

/// <summary>
/// Manages storage for context variables across different repository locations.
/// </summary>
public interface IVariableStorage
{
    /// <summary>
    /// Immutable list of built-in application scoped variables from the <see cref="RepositoryLocation.BuiltIn"/> repository location.
    /// </summary>
    ImmutableList<Variable> BuiltIn { get; set; }

    /// <summary>
    /// List of application scoped variables from the <see cref="RepositoryLocation.Application"/> repository location.
    /// </summary>
    List<Variable> Application { get; }

    /// <summary>
    /// List of session scoped variables from the <see cref="RepositoryLocation.Session"/> repository location.
    /// </summary>
    List<Variable> Session { get; }

    /// <summary>
    /// List of command scoped variables added during command execution. Not persisted.
    /// </summary>
    List<Variable> Command { get; }
}