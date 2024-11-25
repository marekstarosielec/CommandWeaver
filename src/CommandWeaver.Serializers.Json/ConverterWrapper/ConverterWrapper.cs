using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A wrapper for an <see cref="IConverter{TType}"/> implementation, enabling the use of custom converters 
/// within <see cref="System.Text.Json.JsonSerializer"/> operations.
/// </summary>
/// <typeparam name="TType">The type of object to be serialized or deserialized.</typeparam>
internal class ConverterWrapper<TType>(IConverter<TType> converterInstance) : JsonConverter<TType>
{
    /// <summary>
    /// The instance of <see cref="IConverter{TType}"/> used for serialization and deserialization.
    /// </summary>
    private readonly IConverter<TType> _converterInstance = converterInstance ?? throw new ArgumentNullException(nameof(converterInstance));

    /// <summary>
    /// Reads and converts JSON data to an instance of <typeparamref name="TType"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read JSON data from.</param>
    /// <param name="typeToConvert">The type of the object to convert to, specified as <typeparamref name="TType"/>.</param>
    /// <param name="options">Options to customize the deserialization process.</param>
    /// <returns>An instance of <typeparamref name="TType"/> deserialized from the JSON data.</returns>
    public override TType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        _converterInstance.Read(ref reader, typeToConvert, options);

    /// <summary>
    /// Writes the specified <typeparamref name="TType"/> object to JSON format.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to which the JSON data is written.</param>
    /// <param name="value">The <typeparamref name="TType"/> object to serialize.</param>
    /// <param name="options">Options to customize the serialization process.</param>
    public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options) =>
        _converterInstance.Write(writer, value, options);
}
