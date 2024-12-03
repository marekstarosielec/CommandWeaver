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

    /// <summary>
    /// Returns first element matching the predicate, in order of hierarchy - Command, Session, Application, BuiltIn.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Variable? FirstOrDefault(Func<Variable, bool> predicate);

    /// <summary>
    /// Returns first element matching the predicate.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Variable? FirstOrDefault(VariableScope scope, Func<Variable, bool> predicate);
    
    /// <summary>
    /// Removes everything matching predicate in given scope.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="predicate"></param>
    void RemoveAllInScope(VariableScope scope, Predicate<Variable> predicate);
    
    /// <summary>
    /// Removes everything matching predicate in scopes below given in hierarchy - e.g. for scope Application, removes matching in Command and Session.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="predicate"></param>
    void RemoveAllBelowScope(VariableScope scope, Predicate<Variable> predicate);
    
    /// <summary>
    /// Adds variable to list.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="variable"></param>
    void Add(VariableScope scope, Variable variable);
}