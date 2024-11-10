/// <summary>
/// Defines a service for outputting messages to a console or other output target, supporting various log levels.
/// </summary>
public interface IOutput
{
    /// <summary>
    /// Outputs a trace-level message.
    /// </summary>
    /// <param name="message">The trace message to output.</param>
    /// <remarks>Trace messages provide low-level information typically used for detailed tracing during development.</remarks>
    void Trace(string message);

    /// <summary>
    /// Outputs a debug-level message.
    /// </summary>
    /// <param name="message">The debug message to output.</param>
    /// <remarks>Debug messages are used for general debugging information to aid in diagnosing issues.</remarks>
    void Debug(string message);

    /// <summary>
    /// Outputs a warning message.
    /// </summary>
    /// <param name="message">The warning message to output.</param>
    /// <remarks>Warning messages indicate potential issues or noteworthy conditions that do not halt program execution.</remarks>
    void Warning(string message);

    /// <summary>
    /// Outputs an error message.
    /// </summary>
    /// <param name="message">The error message to output.</param>
    /// <remarks>
    /// Error messages indicate serious issues that may halt program execution
    /// </remarks>
    void Error(string message);
}
