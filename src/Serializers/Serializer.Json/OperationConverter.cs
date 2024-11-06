using System.Globalization;
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
            //Skip operation name. 
            if (property.Name.Equals("operation", StringComparison.CurrentCultureIgnoreCase))
                continue;

            //Skip condition.
            if (property.Name.Equals("conditions", StringComparison.CurrentCultureIgnoreCase))
            {
                SetConditions(operationInstance, property.Value);
                continue;
            }
            //Ignore properties starting with "_".
            if (property.Name.StartsWith("_", StringComparison.CurrentCultureIgnoreCase))
                continue;

            if (!operationInstance.Parameters.ContainsKey(property.Name))
            {
                //Property defined in json is not defined in operation class.
                context.Services.Output.Warning($"Property {property.Name} is invalid for operation {operationName} in {context.Variables.CurrentlyProcessedElement}");
                continue;
            }

            operationInstance.Parameters[property.Name] = operationInstance.Parameters[property.Name] with { Value = ReadElement(property.Value) };
        }
    }

    private void SetConditions(Operation operationInstance, JsonElement rootElement)
    {
        foreach (var property in rootElement.EnumerateObject())
        {
            //TODO: Add test if all OperationCondition properties are mapped here.
            if (property.Name.Equals(nameof(OperationCondition.IsNull), StringComparison.InvariantCultureIgnoreCase))
                operationInstance.Conditions.IsNull = ReadElement(property.Value);
            else if (property.Name.Equals(nameof(OperationCondition.IsNotNull), StringComparison.InvariantCultureIgnoreCase))
                operationInstance.Conditions.IsNotNull = ReadElement(property.Value);
            else
                context.Services.Output.Warning($"Unknown condition {property.Name}");
        }
    }
    private DynamicValue ReadElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return ReadString(element);
            case JsonValueKind.Number:
                return ReadNumber(element);
            case JsonValueKind.True:
                return new DynamicValue("true");
            case JsonValueKind.False:
                return new DynamicValue("false");
            case JsonValueKind.Object:
                return ReadObject(element);
            case JsonValueKind.Array:
                return ReadArray(element);
            case JsonValueKind.Null:
                return new DynamicValue(); 
            default:
                throw new JsonException("Unexpected JSON value kind");
        }
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
        var list = new DynamicValueList();
        foreach (var arrayElement in element.EnumerateArray())
        {
            var arrayElementContents = ReadElement(arrayElement);
            var dictionary = arrayElementContents?.ObjectValue;
            dictionary ??= new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", arrayElementContents } });// new VariableValueObject("key", arrayElementContents);
            list.Add(dictionary);
        }
        return new DynamicValue(list);
    }
    
}