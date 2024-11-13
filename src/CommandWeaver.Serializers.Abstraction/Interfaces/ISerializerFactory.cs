/// <summary>
/// Factory interface to obtain the appropriate serializer based on format.
/// </summary>
public interface ISerializerFactory
{
    /// <summary>
    /// Retrieves the appropriate serializer for the specified format.
    /// </summary>
    /// <param name="format">The format for which a serializer is required (e.g., "json", "xml").</param>
    /// <returns>
    /// An implementation of <see cref="ISerializer"/> that can handle the specified format.
    /// Null if the format is not supported.
    /// </returns>
    ISerializer? GetSerializer(string format);

    /// <summary>
    /// Retrieves serializer used for saving changed variable values.
    /// </summary>
    /// <param name="format">The format for which a serializer is used.</param>
    /// <returns></returns>
    ISerializer GetDefaultSerializer(out string format);
}