using System.Text.Json;

/// <summary>
/// Converts a JSON value to a <see cref="DynamicValue"/> instance and vice versa.
/// </summary>
public interface IDynamicValueConverter : IConverter<DynamicValue?>
{ 
    /// <summary>
    /// Reads a <see cref="JsonElement"/> and converts it to a <see cref="DynamicValue"/>.
    /// </summary>
    /// <param name="element">The JSON element to read.</param>
    /// <returns>A <see cref="DynamicValue"/> instance representing the element data.</returns>
    DynamicValue? ReadElement(JsonElement element);
}

/// <inheritdoc />
public class DynamicValueConverter : IDynamicValueConverter
{
    /// <inheritdoc />
    public DynamicValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var element = document.RootElement;
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

    private DynamicValue ReadString(JsonElement element) 
        => element.TryGetDateTimeOffset(out DateTimeOffset dateTimeValue) 
            ? new DynamicValue(dateTimeValue) 
            : new DynamicValue(element.GetString());

    private DynamicValue ReadNumber(JsonElement element) 
        => element.TryGetInt64(out long longValue) 
            ? new DynamicValue(longValue) 
            : new DynamicValue(element.GetDouble());

    private DynamicValue ReadObject(JsonElement element)
    {
        var variable = new Dictionary<string, DynamicValue?>();
        foreach (var property in element.EnumerateObject())
            variable[property.Name] = ReadElement(property.Value);

        return new DynamicValue(variable);
    }

    private DynamicValue ReadArray(JsonElement element)
    {
        var list = element.EnumerateArray().Select(arrayElement => ReadElement(arrayElement) ?? new DynamicValue()).ToList();
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

    private void WriteArray(Utf8JsonWriter writer, DynamicValueList? array, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if (array != null)
            foreach (var element in array)
                Write(writer, element, options);
        writer.WriteEndArray();
    }
}
