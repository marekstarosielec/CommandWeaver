/// <summary>
/// Defines a service that allows influencing the execution flow.
/// </summary>
public interface IFlowService
{
    /// <summary>
    /// Terminates execution, optionally providing an exit message and code.
    /// </summary>
    /// <param name="message">An optional message describing the reason for termination.</param>
    /// <param name="exitCode">An exit code representing the reason for termination; defaults to <c>1</c>.</param>
    /// <remarks>This is typically used to end the flow due to an error.</remarks>
    void Terminate(string? message = null, int exitCode = 1);

    /// <summary>
    /// Handles a non-fatal exception, allowing execution to continue.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <remarks>
    /// Use this to log or manage exceptions that do not require terminating the execution flow.
    /// </remarks>
    void NonFatalException(Exception? exception);

    /// <summary>
    /// Handles a fatal exception and terminates execution.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="message">An optional message providing additional context about the exception.</param>
    /// <param name="exitCode">An exit code representing the reason for termination; defaults to <c>1</c>.</param>
    /// <remarks>
    /// This method should be used for critical errors that prevent further execution of the program.
    /// </remarks>
    void FatalException(Exception? exception, string? message = null, int exitCode = 1);
}