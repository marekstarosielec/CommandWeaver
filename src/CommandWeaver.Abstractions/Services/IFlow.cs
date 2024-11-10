/// <summary>
/// Defines a service that allows influence execution flow.
/// </summary>
public interface IFlow
{
    /// <summary>
    /// Terminates execution, optionally providing an exit message and code.
    /// </summary>
    /// <param name="message">An optional message describing the reason for termination.</param>
    /// <param name="exitCode">An exit code representing the reason for termination; defaults to <c>1</c>.</param>
    /// <remarks>This is typically used to end the flow due to an error.</remarks>
    void Terminate(string? message = null, int exitCode = 1);
}
