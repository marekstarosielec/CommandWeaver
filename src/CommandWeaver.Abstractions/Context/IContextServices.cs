/// <summary>
/// Defines a collection of services available within a specific context.
/// </summary>
public interface IContextServices
{
    /// <summary>
    /// Gets the output service for displaying messages.
    /// </summary>
    /// <remarks>
    /// This service allows for message output at various log levels, such as trace, debug, warning, and error.
    /// </remarks>
    IOutput Output { get; }
}
