using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public class OperationFactory(IServiceProvider serviceProvider, IVariableService variableService, IOutputService outputService , IConditionsService conditionsService) : IOperationFactory
{
    private readonly Dictionary<string, Func<Operation>> _operations = new(StringComparer.OrdinalIgnoreCase)
    {
        { "output", serviceProvider.GetRequiredService<Output> },
        { "base64decode", serviceProvider.GetRequiredService<Base64Decode> },
        { "setVariable", serviceProvider.GetRequiredService<SetVariable> },
        { "terminate", serviceProvider.GetRequiredService<Terminate> },
        { "forEach", serviceProvider.GetRequiredService<ForEach> },
        { "restCall", serviceProvider.GetRequiredService<RestCall> },
        { "restServer", serviceProvider.GetRequiredService<RestServer> },
        { "restServerKill", serviceProvider.GetRequiredService<RestServerKill> },
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
        var resolvedSource = ResolveVariable(source);

        if (resolvedSource.ObjectValue != null)
            return [GetOperation(resolvedSource)];
        if (resolvedSource.ListValue != null)
            return resolvedSource.ListValue.Select(GetOperation);

        throw new CommandWeaverException("Could not resolve operation");
    }

    private Operation GetOperation(DynamicValue source)
    {
        var operationInstance = GetOperationInstanceFromName(source);
        operationInstance = ConfigureOperation(operationInstance, source);
        return operationInstance;
    }

    private DynamicValue ResolveVariable(DynamicValue source)
    {
        if (!string.IsNullOrWhiteSpace(source.ObjectValue?["operation"].TextValue))
            return source;
        
        var resolvedSource = source;
        while (!string.IsNullOrEmpty(resolvedSource.ObjectValue?["fromVariable"].TextValue))
            resolvedSource = variableService.ReadVariableValue(resolvedSource.ObjectValue["fromVariable"], true, 1);

        if (!string.IsNullOrWhiteSpace(resolvedSource?.ObjectValue?["operation"].TextValue)) 
            return resolvedSource;

        if (resolvedSource?.ListValue != null)
        {
            //TODO: Instead of changing existing value, new list should be created. Remove index setter from DynamicValueList.
            for (var x = 0; x < resolvedSource.ListValue.Count; x++)
                resolvedSource.ListValue[x] = ResolveVariable(resolvedSource.ListValue[x]);
            return resolvedSource;
        }

        throw new CommandWeaverException(
            $"Failed to find operation in {source.ObjectValue?["fromVariable"].TextValue}");
    }
    
    private Operation GetOperationInstanceFromName(DynamicValue source)
    {
        var operationName = source.ObjectValue?["operation"].TextValue;
        var operationInstance = GetOperation(operationName);
        if (operationInstance != null) return operationInstance;
        throw new CommandWeaverException($"Unknown operation '{operationName}'");
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
            throw new CommandWeaverException($"Unexpected property '{propertyKey}' in operation '{operationInstance.Name}'");

        parameters[propertyKey] = parameter with { OriginalValue = propertyValue ?? new DynamicValue() };
        outputService.Trace($"Parameter '{propertyKey}' set for operation '{operationInstance.Name}'.");
    }
    
    private void ConfigureSubOperations(Operation operationInstance, DynamicValue source)
    {
        if (operationInstance is not OperationAggregate aggregateInstance)
            throw new CommandWeaverException($"Cannot add operations to operation '{operationInstance.Name}'");

        if (source.ListValue == null)
            throw new CommandWeaverException($"Operations list is missing in '{operationInstance.Name}'");

        //TODO: Add maximum depth here to avoid StackOverflowException
        var subOperations = source.ListValue.Select(GetOperations);
        aggregateInstance.Operations = subOperations.SelectMany(s =>s).ToImmutableList();
    }
}