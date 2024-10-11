using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serializer.Json;

/// <summary>
/// Converts object? to specific type. Without it, it is JsonElement.
/// </summary>
public class ObjectToPureTypeConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.String => ReadString(ref reader),
            JsonTokenType.StartObject => ReadObject(ref reader, typeToConvert, options),
            JsonTokenType.StartArray => ReadArray(ref reader, typeToConvert, options),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unsupported JsonTokenType {reader.TokenType}")
        };

    private object ReadNumber(ref Utf8JsonReader reader)
    {
        // Attempt to get different numeric types
        if (reader.TryGetInt32(out int intValue))
            return intValue.ToString();
        if (reader.TryGetInt64(out long longValue))
            return longValue.ToString();
        if (reader.TryGetDouble(out double doubleValue))
            return doubleValue.ToString(CultureInfo.InvariantCulture);
        return reader.GetDecimal().ToString(CultureInfo.InvariantCulture); // Default to decimal if no other numeric type fits
    }

    private object? ReadString(ref Utf8JsonReader reader)
    {
        // Attempt to parse as DateTime, otherwise return as string
        if (reader.TryGetDateTime(out DateTime dateTimeValue))
            return dateTimeValue.ToString("o");
        return reader.GetString();
    }

    private object ReadObject(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<string, object?>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString() ?? throw new JsonException("Property name cannot be null");
            reader.Read();
            dictionary[propertyName] = Read(ref reader, typeToConvert, options);
        }

        return dictionary;
    }

    private object ReadArray(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<object?>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            list.Add(Read(ref reader, typeToConvert, options));
        
        return list;
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (value)
        {
            case bool boolValue:
                writer.WriteBooleanValue(boolValue);
                break;

            case int intValue:
                writer.WriteNumberValue(intValue);
                break;

            case long longValue:
                writer.WriteNumberValue(longValue);
                break;

            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;

            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;

            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;

            case DateTime dateTimeValue:
                writer.WriteStringValue(dateTimeValue);
                break;

            case IEnumerable<object?> list:
                writer.WriteStartArray();
                foreach (var item in list)
                {
                    Write(writer, item, options);
                }
                writer.WriteEndArray();
                break;

            case IDictionary<string, object?> dictionary:
                writer.WriteStartObject();
                foreach (var kvp in dictionary)
                {
                    writer.WritePropertyName(kvp.Key);
                    Write(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
                break;

            default:
                // Fallback: use JsonSerializer to write complex types
                System.Text.Json.JsonSerializer.Serialize(writer, value, value.GetType(), options);
                break;
        }
    }
}