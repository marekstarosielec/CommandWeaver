/// <summary>
/// Defines a service for writing output, typically to a console or similar output target.
/// </summary>
/// <remarks>
/// This service provides methods for outputting raw text, formatted markup, and JSON data.
/// </remarks>
public interface IOutputWriter
{
    /// <summary>
    /// Writes raw text to the output without any formatting.
    /// </summary>
    /// <param name="textValue">The raw text to write to the output.</param>
    /// <remarks>
    /// This method outputs the text exactly as provided, without applying any formatting or styling.
    /// </remarks>
    void WriteRaw(string textValue);
    
    /// <summary>
    /// Writes formatted markup text to the output.
    /// </summary>
    /// <param name="textValue">The markup text to write to the output.</param>
    /// <remarks>
    /// Markup text may include rich text or other enhanced visual elements, depending on the output implementation.
    /// </remarks>
    void WriteMarkup(string textValue);
    
    /// <summary>
    /// Writes JSON-formatted data to the output.
    /// </summary>
    /// <param name="json">The JSON string to write to the output.</param>
    /// <remarks>
    /// This method ensures the JSON data is correctly formatted and output as-is, suitable for structured or machine-readable logs.
    /// </remarks>
    void WriteJson(string json);

    void WriteException(Exception exception);
}