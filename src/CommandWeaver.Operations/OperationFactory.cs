using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public class OperationFactory(IServiceProvider serviceProvider) : IOperationFactory
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

    /// <inheritdoc />
    public Operation VariableOperation()
        => serviceProvider.GetRequiredService<VariableOperations>();
}