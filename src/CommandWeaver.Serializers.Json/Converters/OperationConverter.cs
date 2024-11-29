using System.Collections.Immutable;
using System.Text.Json;

/// <summary>
/// A JSON converter that converts JSON data to an <see cref="Operation"/> instance using a specified context and factory.
/// </summary>
public interface IOperationConverter : IConverter<Operation>
{ }

/// <inheritdoc />
public class OperationConverter(
    IOutputService outputService, 
    IVariableService variableService, 
    IOperationFactory operationFactory, 
    IFlowService flowService, 
    IConditionsService conditionsService) : IOperationConverter
{
    /// <summary>
    /// A converter for dynamic values within JSON data.
    /// </summary>
    private readonly IDynamicValueConverter _dynamicValueConverter = new DynamicValueConverter();

    /// <inheritdoc />
    public Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var rootElement = document.RootElement;

        // Get and validate operation name
        var operationName = GetOperationName(rootElement);

        // Resolve the operation instance
        var operationInstance = ResolveOperationInstance(operationName);

        // Configure the operation properties from JSON
        operationInstance = ConfigureOperation(operationInstance, rootElement);

        return operationInstance;
    }

    /// <summary>
    /// Writes the specified <see cref="Operation"/> object to JSON.
    /// </summary>
    /// <param name="writer">The writer to which JSON data is written.</param>
    /// <param name="value">The <see cref="Operation"/> instance to write.</param>
    /// <param name="options">Options for JSON serialization.</param>
    public void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options) =>
        throw new InvalidOperationException($"Serializing {nameof(Operation)} is not supported");

    private string GetOperationName(JsonElement element)
    {
        if (element.TryGetProperty("operation", out var operationElement) &&
            operationElement.GetString() is { } operationName) return operationName;
        
        // Operation name is not defined.
        flowService.Terminate($"Operation without name is listed in {variableService.CurrentlyLoadRepositoryElement}");
        return null!;
    }
    
    private Operation ResolveOperationInstance(string operationName)
    {
        if (string.IsNullOrWhiteSpace(operationName))
        {
            flowService.Terminate("Operation name is null or empty in the provided JSON.");
            throw new InvalidOperationException("Operation name cannot be null or empty.");
        }
        
        var operationInstance = operationFactory.GetOperation(operationName);
        if (operationInstance == null)
        {
            flowService.Terminate($"Unknown operation '{operationName}' in {variableService.CurrentlyLoadRepositoryElement}");
            throw new InvalidOperationException($"Operation '{operationName}' could not be resolved.");
        }

        outputService.Trace($"Operation instance resolved for: {operationName}");
        return operationInstance;
    }
    
    private Operation ConfigureOperation(Operation operationInstance, JsonElement rootElement)
    {
        var result = operationInstance;
        var parameters = result.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        foreach (var property in rootElement.EnumerateObject())
            switch (property.Name.ToLowerInvariant())
            {
                case "operation":
                    // Skip the "operation" property
                    break;

                case "conditions":
                    ConfigureConditions(operationInstance, property.Value);
                    break;

                case "comment":
                    operationInstance.Comment = property.Value.GetString();
                    break;

                case "operations":
                    ConfigureSubOperations(operationInstance, property.Value);
                    break;

                default:
                    ConfigureParameter(operationInstance, parameters, property);
                    break;
            
        }
        return result with { Parameters = parameters.ToImmutableDictionary() };
    }

    private void ConfigureConditions(Operation operationInstance, JsonElement conditionsElement)
    {
        var conditionValue = _dynamicValueConverter.ReadElement(conditionsElement);
        if (conditionValue == null)
            return;
        operationInstance.Conditions ??= conditionsService.GetFromDynamicValue(conditionValue);
    }
    
    private void ConfigureSubOperations(Operation operationInstance, JsonElement subOperationsElement)
    {
        if (operationInstance is not OperationAggregate aggregateInstance || subOperationsElement.ValueKind != JsonValueKind.Array)
            return;
        
        var subOperations = subOperationsElement
            .EnumerateArray()
            .Select<JsonElement, Operation>(subOperationElement =>
            {
                var subOperationName = GetOperationName(subOperationElement);
                var subOperationInstance = ResolveOperationInstance(subOperationName);
                subOperationInstance = ConfigureOperation(subOperationInstance, subOperationElement);
                return subOperationInstance;
            })
            .ToImmutableList();

        aggregateInstance.Operations = subOperations;
    }
    
    private void ConfigureParameter(Operation operationInstance, Dictionary<string, OperationParameter> parameters, JsonProperty property)
    {
        if (!parameters.TryGetValue(property.Name, out var parameter))
        {
            flowService.Terminate($"Unexpected property '{property.Name}' in operation '{operationInstance.Name}' in {variableService.CurrentlyLoadRepositoryElement}");
            return; // Will never be reached due to Terminate
        }

        parameters[property.Name] = parameter with { OriginalValue = _dynamicValueConverter.ReadElement(property.Value) ?? new DynamicValue() };
        outputService.Trace($"Parameter '{property.Name}' set for operation '{operationInstance.Name}'.");
    }
}
