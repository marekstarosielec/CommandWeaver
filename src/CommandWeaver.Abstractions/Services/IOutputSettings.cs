/// <summary>
/// Defines settings for output formatting and logging levels, storing values from a variable service.
/// </summary>
/// <remarks>
/// This service is used to manage output styles and settings without directly depending on the variable service, 
/// avoiding circular dependencies. It should be registered as a singleton to ensure consistent configuration 
/// across the application.
/// </remarks>
public interface IOutputSettings
{
    /// <summary>
    /// Gets or sets the style applied to trace-level messages.
    /// </summary>
    /// <remarks>
    /// The style determines how trace-level messages are formatted in the output.
    /// </remarks>
    string? TraceStyle { get; set; }

    /// <summary>
    /// Gets or sets the style applied to debug-level messages.
    /// </summary>
    /// <remarks>
    /// The style determines how debug-level messages are formatted in the output.
    /// </remarks>
    string? DebugStyle { get; set; }

    /// <summary>
    /// Gets or sets the style applied to informational messages.
    /// </summary>
    /// <remarks>
    /// The style determines how informational messages are formatted in the output.
    /// </remarks>
    string? InformationStyle { get; set; }

    /// <summary>
    /// Gets or sets the style applied to warning messages.
    /// </summary>
    /// <remarks>
    /// The style determines how warning messages are formatted in the output.
    /// </remarks>
    string? WarningStyle { get; set; }

    /// <summary>
    /// Gets or sets the style applied to error messages.
    /// </summary>
    /// <remarks>
    /// The style determines how error messages are formatted in the output.
    /// </remarks>
    string? ErrorStyle { get; set; }

    /// <summary>
    /// Gets or sets the current log level for filtering messages.
    /// </summary>
    /// <remarks>
    /// Messages below the current log level will not be output. This controls the verbosity of the logging system.
    /// </remarks>
    LogLevel CurrentLogLevel { get; set; }

    /// <summary>
    /// Gets or sets the serializer used for formatting complex objects in the output.
    /// </summary>
    /// <remarks>
    /// The serializer is used to convert objects to strings, such as when formatting JSON output or other complex data.
    /// </remarks>
    ISerializer? Serializer { get; set; }
}
