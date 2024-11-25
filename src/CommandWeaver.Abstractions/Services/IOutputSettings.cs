using System.Collections.Immutable;

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
    /// Contains list of styles defined in "styles" variable.
    /// </summary>
    ImmutableDictionary<string, string>? Styles { get; set; }
    
    /// <summary>
    /// Sets a <see cref="Styles"/> dictionary from "styles" variable.
    /// </summary>
    /// <param name="styles"></param>
    void SetStyles(DynamicValue styles);
    
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
