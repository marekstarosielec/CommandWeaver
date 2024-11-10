/// <summary>
/// Defines a context that provides access to services, variables, and lifecycle management for executing commands.
/// </summary>
public interface IContext
{
    /// <summary>
    /// Initializes the context asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    Task Initialize(CancellationToken cancellationToken);

    /// <summary>
    /// Runs a specified command within the context.
    /// </summary>
    /// <param name="commmandName">The name of the command to execute.</param>
    /// <param name="arguments">A dictionary of arguments to pass to the command.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous execution of the command.</returns>
    Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken = default);
}
