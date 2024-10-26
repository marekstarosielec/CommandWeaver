using Models;
using System.Collections.Immutable;

namespace Cli2Context;

/// <summary>
/// Manages storage for context variables across different repository locations, 
/// providing immutable lists for BuiltIn, Local, and Session repository locations, 
/// and a mutable list for Changes.
/// </summary>
internal class ContextVariableStorage
{
    /// <summary>
    /// Immutable list of built-in variables from the <see cref="RepositoryLocation.BuiltIn"/> repository location.
     /// </summary>
    public ImmutableList<Variable> BuiltIn { get; set; } = ImmutableList<Variable>.Empty;

    /// <summary>
    /// Immutable list of local variables from the <see cref="RepositoryLocation.Local"/> repository location.
   /// </summary>
    public ImmutableList<Variable> Local { get; set; } = ImmutableList<Variable>.Empty;

    /// <summary>
    /// Immutable list of session variables from the <see cref="RepositoryLocation.Session"/> repository location.
     /// </summary>
    public ImmutableList<Variable> Session { get; set; } = ImmutableList<Variable>.Empty;

    /// <summary>
    /// Mutable list of changes, which can be directly modified and typically holds variables
    /// that are updated or pending to be saved across repository locations.
    /// </summary>
    public List<Variable> Changes { get; } = new List<Variable>();
}
