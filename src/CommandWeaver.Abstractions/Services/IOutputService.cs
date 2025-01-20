/// <summary>
/// Defines a service for outputting messages to a console or other output target, supporting various log levels.
/// </summary>
public interface IOutputService
{
    /// <summary>
    /// Outputs a trace-level message.
    /// </summary>
    /// <param name="message">The trace message to output.</param>
    /// <remarks>
    /// Trace messages provide low-level information typically used for detailed tracing during development.
    /// </remarks>
    void Trace(string message);

    /// <summary>
    /// Outputs a debug-level message.
    /// </summary>
    /// <param name="message">The debug message to output.</param>
    /// <remarks>
    /// Debug messages are used for general debugging information to aid in diagnosing issues.
    /// </remarks>
    void Debug(string message);

    /// <summary>
    /// Outputs an informational message.
    /// </summary>
    /// <param name="message">The informational message to output.</param>
    /// <remarks>
    /// Informational messages provide standard runtime information about the application's state or progress.
    /// </remarks>
    void Information(string message);

    /// <summary>
    /// Outputs a warning message.
    /// </summary>
    /// <param name="message">The warning message to output.</param>
    /// <remarks>
    /// Warning messages indicate potential issues or noteworthy conditions that do not halt program execution.
    /// </remarks>
    void Warning(string message);

    /// <summary>
    /// Outputs an error message.
    /// </summary>
    /// <param name="message">The error message to output.</param>
    /// <remarks>
    /// Error messages indicate serious issues that may halt program execution or require immediate attention.
    /// </remarks>
    void Error(string message);

    /// <summary>
    /// Outputs a dynamic value with an optional log level and styling.
    /// </summary>
    /// <param name="value">The <see cref="DynamicValue"/> to output.</param>
    /// <param name="logLevel">The optional <see cref="LogLevel"/> for the message. If <c>null</c>, a default level is used.</param>
    /// <param name="styling">The <see cref="Styling"/> to apply to the output.</param>
    /// <remarks>
    /// This method allows flexible output of dynamic values with additional logging context and styling options.
    /// </remarks>
    void Write(DynamicValue value, LogLevel? logLevel, Styling styling);
    
    void WriteException(Exception exception);
    
    void WriteRequest(HttpRequestMessage request, string? jsonBody, string? rawBody);
    
    void WriteResponse(HttpResponseMessage response, string? jsonBody, string? rawBody);
}
