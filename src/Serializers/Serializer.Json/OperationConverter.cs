using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using Models.Interfaces;
using Models.Interfaces.Context;

namespace Serializer.Json;

public class OperationConverter(IContext context, IOperationFactory operationFactory) : JsonConverter<Operation>
{
    private readonly ObjectToPureTypeConverter _valueConverter = new();

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

            operationInstance.Parameters[property.Name] = operationInstance.Parameters[property.Name] with { Value = _valueConverter.ReadElement(property.Value) };
        }
    }

    private void SetConditions(Operation operationInstance, JsonElement rootElement)
    {
        foreach (var property in rootElement.EnumerateObject())
        {
            var condition = _valueConverter.ReadElement(property.Value);
            //TODO: Add test if all OperationCondition properties are mapped here.
            if (property.Name.Equals(nameof(OperationCondition.IsNull), StringComparison.InvariantCultureIgnoreCase))
                operationInstance.Conditions.IsNull = condition;
            else if (property.Name.Equals(nameof(OperationCondition.IsNotNull), StringComparison.InvariantCultureIgnoreCase))
                operationInstance.Conditions.IsNotNull = condition;
            else
                context.Services.Output.Warning($"Unknown condition {property.Name}");
        }
    }
}