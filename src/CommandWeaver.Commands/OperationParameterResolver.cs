using System.Collections.Immutable;

/// <summary>
/// Service responsible for preparing and validating operation parameters.
/// </summary>
public interface IOperationParameterResolver
{
    /// <summary>
    /// Prepares operation parameters by resolving their values and validating them.
    /// </summary>
    /// <param name="operation">The operation whose parameters are being prepared.</param>
    /// <returns>A prepared operation with resolved parameters.</returns>
    Operation PrepareOperationParameters(Operation operation);
}

/// <inheritdoc />
internal class OperationParameterResolver(
    IVariableService variableService,
    IFlowService flowService) : IOperationParameterResolver
{
    /// <inheritdoc />
    public Operation PrepareOperationParameters(Operation operation)
    {
        var parameters = operation.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (var parameterKey in operation.Parameters.Keys)
        {
            var parameter = operation.Parameters[parameterKey];

            // Resolve the value from variables
            var resolvedValue = variableService.ReadVariableValue(parameter.OriginalValue);
            parameters[parameterKey] = parameter with { Value = resolvedValue };
            Validate(operation, parameter, resolvedValue, parameterKey);
        }

        return operation with { Parameters = parameters.ToImmutableDictionary() };
    }

    /// <summary>
    /// Checks if parameter meets constraints.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="parameter"></param>
    /// <param name="resolvedValue"></param>
    /// <param name="parameterKey"></param>
    private void Validate(Operation operation, OperationParameter parameter, DynamicValue resolvedValue,
        string parameterKey)
    {
        if (parameter.Required && resolvedValue.IsNull())
            flowService.Terminate($"Parameter '{parameterKey}' is required in operation '{operation.Name}'.");

        if (parameter.RequiredText && string.IsNullOrWhiteSpace(resolvedValue.TextValue))
            flowService.Terminate($"Parameter '{parameterKey}' requires a text value in operation '{operation.Name}'.");

        if (parameter.RequiredList && resolvedValue.ListValue == null)
            flowService.Terminate($"Parameter '{parameterKey}' requires a list value in operation '{operation.Name}'.");

        if (parameter.AllowedEnumValues != null && resolvedValue.TextValue != null &&
            !Enum.GetNames(parameter.AllowedEnumValues).Any(name => name.Equals(resolvedValue.TextValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate($"Parameter '{parameterKey}' has an invalid value in operation '{operation.Name}'.");

        if (parameter.AllowedValues != null && resolvedValue.TextValue != null &&
            !parameter.AllowedValues.Any(value => string.Equals(value, resolvedValue.TextValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate($"Parameter '{parameterKey}' has an invalid value in operation '{operation.Name}'.");
    }
}
