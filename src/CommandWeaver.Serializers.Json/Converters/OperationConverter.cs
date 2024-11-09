using System.Reflection;
using System.Text.Json;

/// <inheritdoc />
public class OperationConverter(IContext context, IOperationFactory operationFactory) : IOperationConverter
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

        if (!rootElement.TryGetProperty("operation", out var operationElement) || operationElement.GetString() is not { } operationName)
        {
            // Operation name is not defined.
            context.Services.Output.Warning(
                $"Operation without name is listed in {context.Variables.CurrentlyProcessedElement}");
            return null;
        }

        var operationInstance = operationFactory.GetOperation(operationName);
        if (operationInstance == null)
        {
            // There is no operation with given name.
            context.Services.Output.Warning($"Operation {operationName} is not valid in {context.Variables.CurrentlyProcessedElement}");
            return null;
        }

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

    /// <summary>
    /// Sets the properties of an <see cref="Operation"/> instance based on JSON data.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="operationInstance">The <see cref="Operation"/> instance to configure.</param>
    /// <param name="rootElement">The root JSON element containing property values.</param>
    private void SetOperationProperties(string operationName, Operation operationInstance, JsonElement rootElement)
    {
        var type = operationInstance.GetType();
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

            // Ignore properties starting with "_".
            if (property.Name.StartsWith("_", StringComparison.CurrentCultureIgnoreCase))
                continue;

            if (!operationInstance.Parameters.ContainsKey(property.Name))
            {
                // Property defined in JSON is not defined in operation class.
                context.Services.Output.Warning($"Property {property.Name} is invalid in operation {operationName} in {context.Variables.CurrentlyProcessedElement}");
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
                conditionProperty.SetValue(operationInstance.Conditions, _dynamicValueConverter.ReadElement(property.Value) ?? new DynamicValue());
            else
                context.Services.Output.Warning($"Unknown condition {property.Name} in operation {operationName} in {context.Variables.CurrentlyProcessedElement}");
    }

    /// <summary>
    /// Cached property information for <see cref="OperationCondition"/>, allowing case-insensitive access.
    /// </summary>
    private static readonly Dictionary<string, PropertyInfo> _conditionProperties = typeof(OperationCondition)
        .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        .ToDictionary(prop => prop.Name, StringComparer.OrdinalIgnoreCase);
}
