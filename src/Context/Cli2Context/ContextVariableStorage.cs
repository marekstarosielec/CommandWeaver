using Models;
using System.Collections.Immutable;

namespace Cli2Context;

/// <summary>
/// Manages storage for context variables across different repository locations.
/// </summary>
internal class ContextVariableStorage
{
    /// <summary>
    /// Immutable list of built-in application scoped variables from the <see cref="RepositoryLocation.BuiltIn"/> repository location.
    /// </summary>
    public ImmutableList<Variable> BuiltIn { get; set; } = ImmutableList<Variable>.Empty;

    /// <summary>
    /// List of application scoped variables from the <see cref="RepositoryLocation.Application"/> repository location.
    /// </summary>
    public List<Variable> Application { get; set; } = new List<Variable>();

    /// <summary>
    /// List of session scoped variables from the <see cref="RepositoryLocation.Session"/> repository location.
    /// </summary>
    public List<Variable> Session { get; set; } = new List<Variable>();

    /// <summary>
    /// List of command scoped variables.
    /// </summary>
    public List<Variable> Command { get; } = new List<Variable>();
}
