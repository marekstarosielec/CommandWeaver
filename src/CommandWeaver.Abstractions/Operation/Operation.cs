/// <summary>
/// Represents an abstract base class for an operation that can be executed within a command.
/// </summary>
public abstract class Operation
{
    /// <summary>
    /// Gets the name of the operation.
    /// </summary>
    /// <remarks>This property uniquely identifies the operation and is used to reference it in commands.</remarks>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the dictionary of parameters associated with this operation.
    /// </summary>
    /// <remarks>
    /// Each parameter is defined by a key and an <see cref="OperationParameter"/> object,
    /// which provides details about the parameter's type, constraints, and requirement status.
    /// </remarks>
    public abstract Dictionary<string, OperationParameter> Parameters { get; }

    /// <summary>
    /// Executes the operation.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation's execution.</param>
    /// <returns>A task representing the asynchronous execution of the operation.</returns>
    public abstract Task Run(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the conditions that determine whether this operation should execute.
    /// </summary>
    /// <remarks>
    /// The <see cref="OperationCondition"/> object defines conditions, such as checking if certain values are null or not,
    /// that control the execution of this operation.
    /// </remarks>
    public OperationCondition Conditions { get; } = new OperationCondition();
}