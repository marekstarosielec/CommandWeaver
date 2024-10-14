using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using Models.Interfaces;
using Models.Interfaces.Context;

namespace Serializer.Json;

public class OperationConverter(IContext context, IOperationFactory operationFactory) : JsonConverter<Operation>
{
    public override Operation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var rootElement = document.RootElement;
        var operationName = rootElement.GetProperty("operation").GetString();
        if (operationName == null)
        {
            //Operation name is not defined.
            context.Services.Output.Warning(
                $"Operation without name is listed in {context.Variables.CurrentlyProcessedElement}");
            return null;
        }
        var operationInstance = operationFactory.GetOperation(operationName);
        if (operationInstance == null)
        {
            //There is no operation with given name.
            context.Services.Output.Warning($"Operation {operationName} is not valid in {context.Variables.CurrentlyProcessedElement}");
            return null;
        }

        SetOperationProperties(operationName, operationInstance, rootElement);
        return operationInstance;
    }

    public override void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options) =>
        System.Text.Json.JsonSerializer.Serialize(writer, value, value.GetType(), options);

    private void SetOperationProperties(string operationName, Operation operationInstance, JsonElement rootElement)
    {
        var type = operationInstance.GetType();
        foreach (var property in rootElement.EnumerateObject())
        {
            //Skip operation name. Read rest of properties.
            if (property.Name.Equals("operation", StringComparison.CurrentCultureIgnoreCase))
                continue;
            
            var propertyInfo = type.GetProperty(property.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                //Property defined in json is not defined in operation class.
                context.Services.Output.Warning($"Property {property.Name} is invalid for operation {operationName} in {context.Variables.CurrentlyProcessedElement}");
                continue;
            }

            if (propertyInfo.GetValue(operationInstance) is not Variable propertyValue)
                //Property is not initialized in operation class.
                throw new InvalidOperationException(
                    $"Property {property.Name} is not initialized in operation {operationName} or has incorrect type");

            propertyValue.Value = ReadElement(property.Value);
        }
    }

    private object? ReadElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return ReadString(element);
            case JsonValueKind.Number:
                return ReadNumber(element);
            case JsonValueKind.True:
                return true.ToString();
            case JsonValueKind.False:
                return false.ToString();
            case JsonValueKind.Object:
                return ReadObject(element);
            case JsonValueKind.Array:
                return ReadArray(element);
            case JsonValueKind.Null:
                return null; 
            default:
                throw new JsonException("Unexpected JSON value kind");
        }
    }
    private object? ReadString(JsonElement element)
    {
        // Attempt to parse as DateTime, otherwise return as string
        if (element.TryGetDateTime(out DateTime dateTimeValue))
            return dateTimeValue.ToString("o");
        return element.GetString();
    }
    
    private object ReadNumber(JsonElement element)
    {
        // Attempt to get different numeric types
        if (element.TryGetInt32(out int intValue))
            return intValue.ToString();
        if (element.TryGetInt64(out long longValue))
            return longValue.ToString();
        if (element.TryGetDouble(out double doubleValue))
            return doubleValue.ToString(CultureInfo.InvariantCulture);
        return element.GetDecimal().ToString(CultureInfo.InvariantCulture); // Default to decimal if no other numeric type fits
    }
    
    private object ReadObject(JsonElement element)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
            dictionary[property.Name] = ReadElement(property.Value);
        
        return dictionary;
    }
    private object ReadArray(JsonElement element)
    {
        var list = new List<Dictionary<string, object?>>();
        foreach (var arrayElement in element.EnumerateArray())
        {
            var arrayElementContents = ReadElement(arrayElement);
            var dictionary = arrayElementContents as Dictionary<string, object?>;
            dictionary ??= new Dictionary<string, object?> { { "key", arrayElementContents } };    
            list.Add(dictionary);
        }
        return list;
    }
    
}