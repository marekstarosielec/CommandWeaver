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
}
