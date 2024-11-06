using Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serializer.Json;

/// <summary>
/// Converts object? to specific type. Without it, it is JsonElement.
/// </summary>
public class ObjectToPureTypeConverter : JsonConverter<DynamicValue?>
{
    public override DynamicValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => new DynamicValue("true"),
            JsonTokenType.False => new DynamicValue("false"),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.String => ReadString(ref reader),
            JsonTokenType.StartObject => ReadObject(ref reader, typeToConvert, options),
            JsonTokenType.StartArray => ReadArray(ref reader, typeToConvert, options),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unsupported JsonTokenType {reader.TokenType}")
        };

    private DynamicValue ReadNumber(ref Utf8JsonReader reader)
    {
        // Attempt to get different numeric types
        if (reader.TryGetInt32(out int intValue))
            return new DynamicValue(intValue.ToString());
        if (reader.TryGetInt64(out long longValue))
            return new DynamicValue(longValue.ToString());
        if (reader.TryGetDouble(out double doubleValue))
            return new DynamicValue(doubleValue.ToString(CultureInfo.InvariantCulture));
        // Default to decimal if no other numeric type fits
        return new DynamicValue(reader.GetDecimal().ToString(CultureInfo.InvariantCulture));
    }

    private DynamicValue? ReadString(ref Utf8JsonReader reader)
    {
        // Attempt to parse as DateTime, otherwise return as string
        if (reader.TryGetDateTime(out DateTime dateTimeValue))
            return new DynamicValue(dateTimeValue.ToString("o"));
        return new DynamicValue(reader.GetString());
    }

    private DynamicValue ReadObject(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var variable = new DynamicValueObject();
       
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString() ?? throw new JsonException("Property name cannot be null");
            reader.Read();
            variable = variable.With(propertyName, Read(ref reader, typeToConvert, options));
        }

        return new DynamicValue(variable);
    }

    private DynamicValue ReadArray(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<DynamicValueObject>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var element = Read(ref reader, typeToConvert, options);
            var dictionaryElement = element?.ObjectValue;
            dictionaryElement ??= new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", element } });
            list.Add(dictionaryElement);
        }
        return new DynamicValue(new DynamicValueList(list));
    }

    public override void Write(Utf8JsonWriter writer, DynamicValue? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}