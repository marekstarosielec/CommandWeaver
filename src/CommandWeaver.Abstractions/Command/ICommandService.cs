/// <summary>
/// Defines a service for handling command definitions and their execution.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Adds a set of commands to the service that can be executed.
    /// </summary>
    /// <param name="commands">The collection of <see cref="Command"/> objects to add.</param>
    void Add(IEnumerable<Command> commands);

    /// <summary>
    /// Validates the commands and their associated operations and parameters.
    /// </summary>
    /// <remarks>
    /// Ensures that all commands are well-formed, have valid operations, and meet the required constraints.
    /// </remarks>
    void Validate();

    /// <summary>
    /// Retrieves a command by its name.
    /// </summary>
    /// <param name="name">The name of the command to retrieve.</param>
    /// <returns>
    /// The <see cref="Command"/> object with the specified name, or <c>null</c> if no matching command is found.
    /// </returns>
    Command? Get(string name);

    /// <summary>
    /// Prepares the parameters for a given command using the provided arguments and saves the values to variables.
    /// </summary>
    /// <param name="command">The command whose parameters are to be prepared.</param>
    /// <param name="arguments">A dictionary of argument names and values, typically provided from command line arguments.</param>
    /// <remarks>
    /// This method processes the provided arguments, matches them to the command's parameters, and stores the resulting
    /// values into variables for later use during command execution.
    /// </remarks>
    void PrepareCommandParameters(Command command, Dictionary<string, string> arguments);

    /// <summary>
    /// Executes a list of operations as part of command execution.
    /// </summary>
    /// <param name="operations">The list of <see cref="Operation"/> objects to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Executes the specified operations in sequence or as defined by their dependencies, respecting the provided cancellation token.
    /// </remarks>
    Task ExecuteOperations(List<Operation> operations, CancellationToken cancellationToken);
}
