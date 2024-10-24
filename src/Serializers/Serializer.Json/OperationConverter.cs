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

    private VariableValue? ReadElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return ReadString(element);
            case JsonValueKind.Number:
                return ReadNumber(element);
            case JsonValueKind.True:
                return new VariableValue("true");
            case JsonValueKind.False:
                return new VariableValue("false");
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
    private VariableValue? ReadString(JsonElement element)
    {
        // Attempt to parse as DateTime, otherwise return as string
        if (element.TryGetDateTime(out DateTime dateTimeValue))
            return new VariableValue(dateTimeValue.ToString("o"));
        return new VariableValue(element.GetString());
    }
    
    private VariableValue ReadNumber(JsonElement element)
    {
        // Attempt to get different numeric types
        if (element.TryGetInt32(out int intValue))
            return new VariableValue(intValue.ToString());
        if (element.TryGetInt64(out long longValue))
            return new VariableValue(longValue.ToString());
        if (element.TryGetDouble(out double doubleValue))
            return new VariableValue(doubleValue.ToString(CultureInfo.InvariantCulture));
        // Default to decimal if no other numeric type fits
        return new VariableValue(element.GetDecimal().ToString(CultureInfo.InvariantCulture));
    }
    
    private VariableValue ReadObject(JsonElement element)
    {
        var variable = new VariableValueObject();
        foreach (var property in element.EnumerateObject())
            variable = variable.With(property.Name, ReadElement(property.Value));
        
        return new VariableValue(variable);
    }
    private VariableValue ReadArray(JsonElement element)
    {
        var list = new VariableValueList();
        foreach (var arrayElement in element.EnumerateArray())
        {
            var arrayElementContents = ReadElement(arrayElement);
            var dictionary = arrayElementContents?.ObjectValue;
            dictionary ??= new VariableValueObject("key", arrayElementContents);
            list.Add(dictionary);
        }
        return new VariableValue(list);
    }
    
}