using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public class OperationFactory(IServiceProvider serviceProvider, IVariableService variableService, IFlowService flowService, IOutputService outputService , IConditionsService conditionsService) : IOperationFactory
{
    private readonly Dictionary<string, Func<Operation>> _operations = new(StringComparer.OrdinalIgnoreCase)
    {
        { "output", serviceProvider.GetRequiredService<Output> },
        { "setVariable", serviceProvider.GetRequiredService<SetVariable> },
        { "terminate", serviceProvider.GetRequiredService<Terminate> },
        { "forEach", serviceProvider.GetRequiredService<ForEach> },
        { "restCall", serviceProvider.GetRequiredService<RestCall> },
        { "block", serviceProvider.GetRequiredService<Block> },
        { "extractFromNameValue", serviceProvider.GetRequiredService<ExtractFromNameValue> },
        { "listGroup", serviceProvider.GetRequiredService<ListGroup> }
    };

    /// <inheritdoc />
    public Operation? GetOperation(string? name) =>
        !string.IsNullOrWhiteSpace(name) && _operations.TryGetValue(name, out var factory) 
            ? factory() 
            : null;
    
    /// <inheritdoc />
    public Dictionary<string, Operation> GetOperations() =>
        _operations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value(), StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Operation> GetOperations(DynamicValue source)
    {
        if (source.ObjectValue != null)
            return [GetOperation(source)];
        // if (source.ListValue != null)
        //     return [GetOperation(source)];
        throw new NotImplementedException();
    }

    private Operation GetOperation(DynamicValue source)
    {
        ArgumentNullException.ThrowIfNull(source.ObjectValue);
        var resolvedSource = ResolveVariable(source);
        var operationInstance = GetOperationInstanceFromName(resolvedSource);
        operationInstance = ConfigureOperation(operationInstance, resolvedSource);
        return operationInstance;
    }

    private DynamicValue ResolveVariable(DynamicValue source)
    {
        if (source.ObjectValue?.ContainsKey("operation") == true && !string.IsNullOrWhiteSpace(source.ObjectValue?["operation"].TextValue))
            return source;
        
        var resolvedSource = source;
        while (resolvedSource.ObjectValue?.ContainsKey("fromVariable") == true && !string.IsNullOrEmpty(resolvedSource.ObjectValue?["fromVariable"].TextValue))
            resolvedSource = variableService.ReadVariableValue(resolvedSource.ObjectValue["fromVariable"], false, 1);

        if (!string.IsNullOrWhiteSpace(resolvedSource?.ObjectValue?["operation"].TextValue)) 
            return resolvedSource;
        
        flowService.Terminate(
            $"Failed to find operation in {source.ObjectValue?["fromVariable"].TextValue}");
        throw new Exception($"Failed to find operation in {source.ObjectValue?["fromVariable"].TextValue}");

    }
    
    private Operation GetOperationInstanceFromName(DynamicValue source)
    {
        var operationName = source.ObjectValue?["operation"].TextValue;
        var operationInstance = GetOperation(operationName);
        if (operationInstance != null) return operationInstance;
        flowService.Terminate($"Unknown operation '{operationName}'");
        throw new Exception($"Unknown operation '{operationName}'");
    }

    private Operation ConfigureOperation(Operation operationInstance, DynamicValue source)
    {
        var result = operationInstance;
        var parameters = result.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        foreach (var propertyKey in source.ObjectValue?.Keys ?? [])
        {
            switch (propertyKey.ToLowerInvariant())
            {
                case "operation":
                    // Skip the "operation" property, it was used to create instance
                    break;

                case "conditions":
                    operationInstance.Conditions ??= conditionsService.GetFromDynamicValue(source.ObjectValue![propertyKey]);
                    break;

                case "comment":
                    operationInstance.Comment = source.ObjectValue![propertyKey];
                    break;

                case "enabled":
                    operationInstance.Enabled = source.ObjectValue![propertyKey];
                    break;

                case "operations":
                    ConfigureSubOperations(operationInstance, source.ObjectValue![propertyKey]);
                    break;

                default:
                    ConfigureParameter(operationInstance, parameters, propertyKey, source.ObjectValue![propertyKey]);
                    break;

            }
        }

        return result with { Parameters = parameters.ToImmutableDictionary() };
    }
    
    private void ConfigureParameter(Operation operationInstance, Dictionary<string, OperationParameter> parameters, string propertyKey, DynamicValue? propertyValue)
    {
        if (!parameters.TryGetValue(propertyKey, out var parameter))
        {
            flowService.Terminate($"Unexpected property '{propertyKey}' in operation '{operationInstance.Name}'");
            return; // Will never be reached due to Terminate
        }

        parameters[propertyKey] = parameter with { OriginalValue = propertyValue ?? new DynamicValue() };
        outputService.Trace($"Parameter '{propertyKey}' set for operation '{operationInstance.Name}'.");
    }
    
    private void ConfigureSubOperations(Operation operationInstance, DynamicValue source)
    {
        if (operationInstance is not OperationAggregate aggregateInstance)
        {
            flowService.Terminate($"Cannot add operations to operation '{operationInstance.Name}'");
            return;
        }

        if (source.ListValue == null)
        {
            flowService.Terminate($"Operations list is missing in '{operationInstance.Name}'");
            return;
        }
        var subOperations = source.ListValue
            .Select<DynamicValue, Operation>(subOperationElement =>
            {
                var resolvedSubOperation = ResolveVariable(subOperationElement);
                var subOperationInstance = GetOperationInstanceFromName(resolvedSubOperation);
                subOperationInstance = ConfigureOperation(subOperationInstance, resolvedSubOperation);
                return subOperationInstance;
            })
            .ToImmutableList();
        
        aggregateInstance.Operations = subOperations;
    }
}