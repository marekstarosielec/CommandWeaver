using System.Collections.Immutable;

/// <summary>
/// Represents an abstract base record for an operation that can be executed within a command.
/// </summary>
public abstract record Operation
{
    /// <summary>
    /// Gets the name of the operation.
    /// </summary>
    /// <remarks>
    /// This property uniquely identifies the operation and is used to reference it in commands.
    /// </remarks>
    public abstract string Name { get; }

    /// <summary>
    /// Optional comment for an operation.
    /// </summary>
    public string? Comment { get; set; }
    
    /// <summary>
    /// Change to false to disable operation.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the dictionary of parameters associated with this operation.
    /// </summary>
    /// <remarks>
    /// Each parameter is defined by a key and an <see cref="OperationParameter"/> object, which provides details
    /// about the parameter's type, constraints, and requirement status.
    /// </remarks>
    public abstract ImmutableDictionary<string, OperationParameter> Parameters { get; init; }

    /// <summary>
    /// Executes the operation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation's execution.</param>
    /// <returns>A task representing the asynchronous execution of the operation.</returns>
    /// <remarks>
    /// The operation performs its specific task when invoked, respecting the provided cancellation token.
    /// </remarks>
    public abstract Task Run(CancellationToken cancellationToken);

    /// <summary>
    /// Gets or sets the conditions that determine whether this operation should execute.
    /// </summary>
    /// <remarks>
    /// The <see cref="Condition"/> object defines rules or constraints that control whether this operation
    /// should be executed. For example, a condition might specify that the operation runs only if
    /// certain variables meet specific criteria.
    /// </remarks>
    public Condition? Conditions { get; set; }
}