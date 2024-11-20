using System.Reflection;
using System.Text.Json;

/// <inheritdoc />
public class OperationConverter(IVariables variables, IOperationFactory operationFactory, IFlow flow) : IOperationConverter
{
    /// <summary>
    /// A converter for dynamic values within JSON data.
    /// </summary>
    internal readonly IDynamicValueConverter _dynamicValueConverter = new DynamicValueConverter();

    /// <inheritdoc />
    public Operation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var rootElement = document.RootElement;
        var operationName = GetOperationName(rootElement);
        var operationInstance = GetOperationInstance(operationName);
        SetOperationProperties(operationName, operationInstance, rootElement);
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
        if (!element.TryGetProperty("operation", out var operationElement) || operationElement.GetString() is not { } operationName)
        {
            // Operation name is not defined.
            flow.Terminate(
                $"Operation without name is listed in {variables.CurrentlyLoadRepositoryElement}");
            return null!;
        }
        return operationName;
    }
    
    private Operation GetOperationInstance(string operationName)
    {
        var result = operationFactory.GetOperation(operationName);
        if (result != null)
            return result;
        
        // There is no operation with given name.
        flow.Terminate($"Operation {operationName} is not valid in {variables.CurrentlyLoadRepositoryElement}");
        return default!;
    }
    
    /// <summary>
    /// Sets the properties of an <see cref="Operation"/> instance based on JSON data.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="operationInstance">The <see cref="Operation"/> instance to configure.</param>
    /// <param name="rootElement">The root JSON element containing property values.</param>
    private void SetOperationProperties(string operationName, Operation operationInstance, JsonElement rootElement)
    {
        foreach (var property in rootElement.EnumerateObject())
        {
            // Skip operation name. 
            if (property.Name.Equals("operation", StringComparison.CurrentCultureIgnoreCase))
                continue;

            // Skip condition.
            if (property.Name.Equals("conditions", StringComparison.CurrentCultureIgnoreCase))
            {
                SetConditions(operationName, operationInstance, property.Value);
                continue;
            }
            
            // Set sub operations.
            if (property.Name.Equals("operations", StringComparison.CurrentCultureIgnoreCase) 
                && operationInstance is OperationAggregate operationAggregateInstance
                && property.Value.ValueKind == JsonValueKind.Array)
            {
                //Keeping operations in serialized form, because they might be needed several times (e.g. to run in loop)
                //and they need to be cloned.
                operationAggregateInstance.SerializedOperations = property.Value.GetRawText();
                continue;
            }

            // Ignore properties starting with "_".
            if (property.Name.StartsWith("_", StringComparison.CurrentCultureIgnoreCase))
                continue;

            if (!operationInstance.Parameters.ContainsKey(property.Name))
            {
                // Property defined in JSON is not defined in operation class.
                flow.Terminate($"Property {property.Name} is invalid in operation {operationName} in {variables.CurrentlyLoadRepositoryElement}");
                continue;
            }

            operationInstance.Parameters[property.Name] = operationInstance.Parameters[property.Name] with { Value = _dynamicValueConverter.ReadElement(property.Value) ?? new DynamicValue() };
        }
    }

    /// <summary>
    /// Sets the conditions of an <see cref="Operation"/> instance based on JSON data.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="operationInstance">The <see cref="Operation"/> instance to configure conditions for.</param>
    /// <param name="rootElement">The JSON element containing condition properties.</param>
    private void SetConditions(string operationName, Operation operationInstance, JsonElement rootElement)
    {
        foreach (var property in rootElement.EnumerateObject())
            if (_conditionProperties.TryGetValue(property.Name, out var conditionProperty))
            {
                operationInstance.Conditions ??= new Condition();
                conditionProperty.SetValue(operationInstance.Conditions, _dynamicValueConverter.ReadElement(property.Value) ?? new DynamicValue());
            }
            else
                flow.Terminate($"Unknown condition {property.Name} in operation {operationName} in {variables.CurrentlyLoadRepositoryElement}");
    }

    /// <summary>
    /// Cached property information for <see cref="Condition"/>, allowing case-insensitive access.
    /// </summary>
    private static readonly Dictionary<string, PropertyInfo> _conditionProperties = typeof(Condition)
        .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        .ToDictionary(prop => prop.Name, StringComparer.OrdinalIgnoreCase);
}
