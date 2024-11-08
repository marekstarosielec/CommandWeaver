using Models;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Serializer.Json;

/// <summary>
/// Converts object? to specific type. Without it, it is JsonElement.
/// </summary>
public class ObjectToPureTypeConverter : JsonConverter<DynamicValue?>
{
    public override DynamicValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement element = document.RootElement;
        return ReadElement(element);
    }

    public DynamicValue? ReadElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.True => new DynamicValue("true"),
            JsonValueKind.False => new DynamicValue("false"),
            JsonValueKind.Number => ReadNumber(element),
            JsonValueKind.String => ReadString(element),
            JsonValueKind.Object => ReadObject(element),
            JsonValueKind.Array => ReadArray(element),
            JsonValueKind.Null => null,
            _ => throw new JsonException($"Unsupported JsonValueKind {element.ValueKind}")
        };
    }

    private DynamicValue ReadString(JsonElement element)
    {
        // Attempt to parse as DateTime, otherwise return as string
        if (element.TryGetDateTime(out DateTime dateTimeValue))
            return new DynamicValue(dateTimeValue.ToString("o"));
        return new DynamicValue(element.GetString());
    }

    private DynamicValue ReadNumber(JsonElement element)
    {
        // Attempt to get different numeric types
        if (element.TryGetInt32(out int intValue))
            return new DynamicValue(intValue.ToString());
        if (element.TryGetInt64(out long longValue))
            return new DynamicValue(longValue.ToString());
        if (element.TryGetDouble(out double doubleValue))
            return new DynamicValue(doubleValue.ToString(CultureInfo.InvariantCulture));
        // Default to decimal if no other numeric type fits
        return new DynamicValue(element.GetDecimal().ToString(CultureInfo.InvariantCulture));
    }

    private DynamicValue ReadObject(JsonElement element)
    {
        var variable = new DynamicValueObject();
        foreach (var property in element.EnumerateObject())
            variable = variable.With(property.Name, ReadElement(property.Value));

        return new DynamicValue(variable);
    }
    private DynamicValue ReadArray(JsonElement element)
    {
        var list = new List<DynamicValueObject>();
        foreach (var arrayElement in element.EnumerateArray())
        {
            var arrayElementContents = ReadElement(arrayElement);
            var dictionary = arrayElementContents?.ObjectValue;
            dictionary ??= new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", arrayElementContents } });// new VariableValueObject("key", arrayElementContents);
            list.Add(dictionary);
        }
        return new DynamicValue(new DynamicValueList(list));
    }

    public override void Write(Utf8JsonWriter writer, DynamicValue? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}