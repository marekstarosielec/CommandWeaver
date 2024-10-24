using Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serializer.Json;

/// <summary>
/// Converts object? to specific type. Without it, it is JsonElement.
/// </summary>
public class ObjectToPureTypeConverter : JsonConverter<VariableValue?>
{
    public override VariableValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => new VariableValue("true"),
            JsonTokenType.False => new VariableValue("false"),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.String => ReadString(ref reader),
            JsonTokenType.StartObject => ReadObject(ref reader, typeToConvert, options),
            JsonTokenType.StartArray => ReadArray(ref reader, typeToConvert, options),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unsupported JsonTokenType {reader.TokenType}")
        };

    private VariableValue ReadNumber(ref Utf8JsonReader reader)
    {
        // Attempt to get different numeric types
        if (reader.TryGetInt32(out int intValue))
            return new VariableValue(intValue.ToString());
        if (reader.TryGetInt64(out long longValue))
            return new VariableValue(longValue.ToString());
        if (reader.TryGetDouble(out double doubleValue))
            return new VariableValue(doubleValue.ToString(CultureInfo.InvariantCulture));
        // Default to decimal if no other numeric type fits
        return new VariableValue(reader.GetDecimal().ToString(CultureInfo.InvariantCulture));
    }

    private VariableValue? ReadString(ref Utf8JsonReader reader)
    {
        // Attempt to parse as DateTime, otherwise return as string
        if (reader.TryGetDateTime(out DateTime dateTimeValue))
            return new VariableValue(dateTimeValue.ToString("o"));
        return new VariableValue(reader.GetString());
    }

    private VariableValue ReadObject(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var variable = new VariableValueObject();
       
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString() ?? throw new JsonException("Property name cannot be null");
            reader.Read();
            variable = variable.With(propertyName, Read(ref reader, typeToConvert, options));
        }

        return new VariableValue(variable);
    }

    private VariableValue ReadArray(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new VariableValueList();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var element = Read(ref reader, typeToConvert, options);
            var dictionaryElement = element?.ObjectValue;
            dictionaryElement ??= new VariableValueObject("key", element);
            list.Add(dictionaryElement);
        }
        return new VariableValue(list);
    }

    public override void Write(Utf8JsonWriter writer, VariableValue? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}