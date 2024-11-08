// ReSharper disable CheckNamespace

namespace Serializer.Abstractions;

/// <summary>
/// Interface for serialization operations.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Deserializes the input string into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the input string should be deserialized.</typeparam>
    /// <param name="input">The string representation of the data to be deserialized.</param>
    /// <returns>
    /// The deserialized object of type <typeparamref name="T"/>, or <c>null</c> if deserialization fails.
    /// </returns>
    T? Deserialize<T>(string input) where T : class;

    string Serialize<T>(T? value) where T : class;
}