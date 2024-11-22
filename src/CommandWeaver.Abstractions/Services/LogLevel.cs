/// <summary>
/// Specifies the severity level of a log message.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Represents detailed diagnostic information for tracing program execution.
    /// </summary>
    Trace,

    /// <summary>
    /// Represents diagnostic information used for debugging.
    /// </summary>
    Debug,

    /// <summary>
    /// Represents general runtime information about the application's state or progress.
    /// </summary>
    Information,

    /// <summary>
    /// Represents a warning about a potential issue or noteworthy condition that does not halt execution.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents an error or critical issue that requires immediate attention.
    /// </summary>
    Error
}