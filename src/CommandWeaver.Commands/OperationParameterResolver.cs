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
    IFlowService flowService,
    IValidationService validationService) : IOperationParameterResolver
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
            validationService.Validate(parameter.Validation, resolvedValue, parameterKey); 
        }

        return operation with { Parameters = parameters.ToImmutableDictionary() };
    }
}
