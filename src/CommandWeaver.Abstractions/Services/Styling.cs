/// <summary>
/// Specifies the styling options for formatting output messages.
/// </summary>
public enum Styling
{
    /// <summary>
    /// Formats the output using markup styling, which may include rich text or other enhanced visual elements.
    /// </summary>
    Markup,

    /// <summary>
    /// Formats the output using markup styling, ensuring it is displayed as a single line.
    /// </summary>
    MarkupLine,

    /// <summary>
    /// Formats the output as JSON, providing structured and machine-readable output.
    /// </summary>
    Json,

    /// <summary>
    /// Outputs the content exactly as it is, without any additional formatting.
    /// </summary>
    Raw,

    /// <summary>
    /// Applies the default styling option, typically determined by the output service implementation.
    /// </summary>
    Default
}