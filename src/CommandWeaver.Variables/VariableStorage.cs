using System.Collections.Immutable;

/// <inheritdoc />
public class VariableStorage : IVariableStorage
{
    /// <inheritdoc />
    public ImmutableList<Variable> BuiltIn { get; set; } = ImmutableList<Variable>.Empty;

    /// <inheritdoc />
    public List<Variable> Application { get; } = new ();

    /// <inheritdoc />
    public List<Variable> Session { get; } = new ();

    /// <inheritdoc />
    public List<Variable> Command { get; } = new ();

    /// <inheritdoc />
    public Variable? FirstOrDefault(Func<Variable, bool> predicate) 
        => Command.FirstOrDefault(predicate) 
           ?? Session.FirstOrDefault(predicate) 
           ?? Application.FirstOrDefault(predicate) 
           ?? BuiltIn.FirstOrDefault(predicate);

    /// <inheritdoc />
    public Variable? FirstOrDefault(VariableScope scope, Func<Variable, bool> predicate) =>
        scope switch
        {
            VariableScope.Command => Command.FirstOrDefault(predicate),
            VariableScope.Session => Session.FirstOrDefault(predicate),
            VariableScope.Application => Application.FirstOrDefault(predicate) ?? BuiltIn.FirstOrDefault(predicate),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };

    /// <inheritdoc />
    public void RemoveAllInScope(VariableScope scope, Predicate<Variable> predicate)
    {
        switch (scope)
        {
            case VariableScope.Command: 
                Command.RemoveAll(predicate);
                break;
            case VariableScope.Session:
                Session.RemoveAll(predicate);
                break;
            case VariableScope.Application:
                Application.RemoveAll(predicate);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
        };
    }

    /// <inheritdoc />
    public void RemoveAllBelowScope(VariableScope scope, Predicate<Variable> predicate)
    {
        switch (scope)
        {
            case VariableScope.Command: 
                break;
            case VariableScope.Session:
                Command.RemoveAll(predicate);
                break;
            case VariableScope.Application:
                Command.RemoveAll(predicate);
                Session.RemoveAll(predicate);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
        };
    }

    /// <inheritdoc />
    public void Add(VariableScope scope, Variable variable)
    {
        switch (scope)
        {
            case VariableScope.Command:
                Command.Add(variable);
                break;
            case VariableScope.Session:
                Session.Add(variable);
                break;
            case VariableScope.Application:
                Application.Add(variable);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
        }
    }
}
