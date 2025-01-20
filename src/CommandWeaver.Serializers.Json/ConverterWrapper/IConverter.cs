using System.Text.Json;

/// <summary>
/// Defines methods for converting objects of type <typeparamref name="TType"/> 
/// to and from JSON using a custom converter implementation.
/// </summary>
/// <typeparam name="TType">The type of object to be converted.</typeparam>
public interface IConverter<TType>
{
    /// <summary>
    /// Reads JSON data and converts it to an instance of <typeparamref name="TType"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read JSON data from.</param>
    /// <param name="typeToConvert">The type of the object to convert to, specified as <typeparamref name="TType"/>.</param>
    /// <param name="options">Options to customize the deserialization process.</param>
    /// <returns>An instance of <typeparamref name="TType"/> deserialized from the JSON data, or <c>null</c> if deserialization fails.</returns>
    TType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

    /// <summary>
    /// Writes an instance of <typeparamref name="TType"/> to JSON format.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which JSON data is written.</param>
    /// <param name="value">The <typeparamref name="TType"/> object to serialize.</param>
    /// <param name="options">Options to customize the serialization process.</param>
    void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options);
}
