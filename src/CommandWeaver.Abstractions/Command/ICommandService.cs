/// <summary>
/// Defines a service for handling command definitions and their execution.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Adds a set of commands to the service that can be executed.
    /// </summary>
    /// <param name="repositoryElementId">The unique identifier of the repository element.</param>
    /// <param name="commands">The collection of <see cref="Command"/> objects to add.</param>
    void Add(string repositoryElementId, IEnumerable<Command> commands);

    /// <summary>
    /// Retrieves a command by its name.
    /// </summary>
    /// <param name="name">The name of the command to retrieve.</param>
    /// <returns>
    /// The <see cref="Command"/> object with the specified name, or <c>null</c> if no matching command is found.
    /// </returns>
    Command? Get(string name);

    /// <summary>
    /// Executes a list of operations as part of command execution.
    /// </summary>
    /// <param name="operations">The enumerable of <see cref="Operation"/> objects to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Executes the specified operations in sequence or as defined by their dependencies, respecting the provided cancellation token.
    /// </remarks>
    Task ExecuteOperations(IEnumerable<Operation> operations, CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes a list of operations as part of command execution.
    /// </summary>
    /// <param name="operations">The enumerable of <see cref="Operation"/> objects to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Executes the specified operations in sequence or as defined by their dependencies, respecting the provided cancellation token.
    /// </remarks>
    Task ExecuteOperations(IEnumerable<DynamicValue> operations, CancellationToken cancellationToken);
}
