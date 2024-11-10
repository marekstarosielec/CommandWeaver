/// <summary>
/// Defines a context that provides access to services, variables, and lifecycle management for executing commands.
/// </summary>
public interface IContext
{
    /// <summary>
    /// Gets the context services available within this context.
    /// </summary>
    /// <remarks>This includes services for outputting messages, logging, and other context-specific functionalities.</remarks>
   // IContextServices Services { get; }

    /// <summary>
    /// Gets the context variables available within this context.
    /// </summary>
    /// <remarks>Provides access to variables that can be set, retrieved, and resolved within the context.</remarks>
    IVariables Variables { get; }

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

    /// <summary>
    /// Terminates the context, optionally providing an exit message and code.
    /// </summary>
    /// <param name="message">An optional message describing the reason for termination.</param>
    /// <param name="exitCode">An exit code representing the reason for termination; defaults to <c>1</c>.</param>
    /// <remarks>This is typically used to end the context due to an error or when execution is complete.</remarks>
    void Terminate(string? message = null, int exitCode = 1);
}
