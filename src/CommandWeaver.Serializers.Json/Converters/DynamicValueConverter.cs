using System.Text.Json;

/// <inheritdoc />
public class DynamicValueConverter : IDynamicValueConverter
{
    /// <inheritdoc />
    public DynamicValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement element = document.RootElement;
        return ReadElement(element);
    }

    /// <inheritdoc />
    public DynamicValue? ReadElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.True => new DynamicValue(true),
        JsonValueKind.False => new DynamicValue(false),
        JsonValueKind.Number => ReadNumber(element),
        JsonValueKind.String => ReadString(element),
        JsonValueKind.Object => ReadObject(element),
        JsonValueKind.Array => ReadArray(element),
        JsonValueKind.Null => new DynamicValue(),
        _ => throw new JsonException($"Unsupported JsonValueKind {element.ValueKind}")
    };

    /// <summary>
    /// Reads a JSON string element, attempting to parse it as a <see cref="DateTimeOffset"/>.
    /// If unsuccessful, returns the value as a string.
    /// </summary>
    /// <param name="element">The JSON element containing a string.</param>
    /// <returns>A <see cref="DynamicValue"/> representing the string or date-time value.</returns>
    private DynamicValue ReadString(JsonElement element)
    {
        if (element.TryGetDateTimeOffset(out DateTimeOffset dateTimeValue))
            return new DynamicValue(dateTimeValue);
        return new DynamicValue(element.GetString());
    }

    /// <summary>
    /// Reads a JSON number element, attempting to parse it as <c>long</c> or <c>double</c> in that order.
    /// </summary>
    /// <param name="element">The JSON element containing a number.</param>
    /// <returns>A <see cref="DynamicValue"/> representing the numeric value.</returns>
    private DynamicValue ReadNumber(JsonElement element)
    {
        if (element.TryGetInt64(out long longValue))
            return new DynamicValue(longValue);
        return new DynamicValue(element.GetDouble());
    }

    /// <summary>
    /// Reads a JSON object and converts it to a <see cref="DynamicValue"/> containing an object.
    /// </summary>
    /// <param name="element">The JSON object element.</param>
    /// <returns>A <see cref="DynamicValue"/> containing the object.</returns>
    private DynamicValue ReadObject(JsonElement element)
    {
        var variable = new Dictionary<string, DynamicValue?>();
        foreach (var property in element.EnumerateObject())
            variable[property.Name] = ReadElement(property.Value);

        return new DynamicValue(variable);
    }

    /// <summary>
    /// Reads a JSON array and converts it to a <see cref="DynamicValue"/> containing a list.
    /// </summary>
    /// <param name="element">The JSON array element.</param>
    /// <returns>A <see cref="DynamicValue"/> containing the array.</returns>
    private DynamicValue ReadArray(JsonElement element)
    {
        var list = new List<DynamicValueObject>();
        foreach (var arrayElement in element.EnumerateArray())
        {
            var arrayElementContents = ReadElement(arrayElement);
            var dictionary = arrayElementContents?.ObjectValue;
            dictionary ??= new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", arrayElementContents } });
            list.Add(dictionary);
        }
        return new DynamicValue(list);
    }

    /// <summary>
    /// Writes a <see cref="DynamicValue"/> to JSON.
    /// </summary>
    /// <param name="writer">The writer to which JSON data is written.</param>
    /// <param name="value">The <see cref="DynamicValue"/> instance to write.</param>
    /// <param name="options">Serialization options.</param>
    public void Write(Utf8JsonWriter writer, DynamicValue? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        //TODO: Test checking if all kind of values from DynamicValue are created.
        if (value.TextValue != null) writer.WriteStringValue(value.TextValue);
        else if (value.DateTimeValue.HasValue) writer.WriteStringValue(value.DateTimeValue.Value);
        else if (value.BoolValue.HasValue) writer.WriteBooleanValue(value.BoolValue.Value);
        else if (value.NumericValue.HasValue) writer.WriteNumberValue(value.NumericValue.Value);
        else if (value.PrecisionValue.HasValue) writer.WriteNumberValue(value.PrecisionValue.Value);
        else if (value.ObjectValue != null) WriteObject(writer, value.ObjectValue, options);
        else if (value.ListValue != null) WriteArray(writer, value.ListValue, options);
        else writer.WriteNullValue();
    }

    /// <summary>
    /// Writes a <see cref="DynamicValueObject"/> to JSON as a JSON object.
    /// </summary>
    /// <param name="writer">The writer to which JSON data is written.</param>
    /// <param name="obj">The <see cref="DynamicValueObject"/> to write.</param>
    /// <param name="options">Serialization options.</param>
    private void WriteObject(Utf8JsonWriter writer, DynamicValueObject? obj, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (obj != null)
            foreach (var property in obj.Keys)
            {
                writer.WritePropertyName(property);
                Write(writer, obj[property], options);
            }
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a <see cref="DynamicValueList"/> to JSON as a JSON array.
    /// </summary>
    /// <param name="writer">The writer to which JSON data is written.</param>
    /// <param name="array">The <see cref="DynamicValueList"/> to write.</param>
    /// <param name="options">Serialization options.</param>
    private void WriteArray(Utf8JsonWriter writer, DynamicValueList? array, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if (array != null)
            foreach (var element in array)
                WriteObject(writer, element, options);
        writer.WriteEndArray();
    }
}
