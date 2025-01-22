/// <summary>
/// Interface for serialization operations.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Attempts to deserialize the specified content to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="content">The serialized content.</param>
    /// <param name="result">The deserialized object if successful; otherwise, <c>null</c>.</param>
    /// <param name="exception">The exception if deserialization fails; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization is successful; otherwise, <c>false</c>.</returns>
    bool TryDeserialize<T>(string content, out T? result, out Exception? exception) where T : class;

    /// <summary>
    /// Attempts to serialize the specified object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="result">The serialized string if successful; otherwise, <c>null</c>.</param>
    /// <param name="exception">The exception if serialization fails; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if serialization is successful; otherwise, <c>false</c>.</returns>
    bool TrySerialize<T>(T? value, out string? result, out Exception? exception) where T : class;
}