using System.Text.Json;

/// <inheritdoc />
public class CommandService(
    IConditionsService conditionsService,
    ICommandMetadataService commandMetadataService,
    IOperationParameterResolver operationParameterResolver,
    IOutputService outputService,
    IOperationFactory operationFactory,
    IVariableService variableService) : ICommandService
{
    private readonly List<Command> _commands = [];

    /// <inheritdoc />
    public void Add(string repositoryElementId, IEnumerable<Command> commands)
    {
        var commandList = commands.ToList();
        _commands.AddRange(commandList);

        foreach (var command in commandList)
            commandMetadataService.StoreCommandMetadata(repositoryElementId, command);
    }

    /// <inheritdoc />
    public Command? Get(string name) =>
        _commands.FirstOrDefault(c => c.GetAllNames().Any(n => n == name) == true);
    
    public async Task ExecuteOperations(IEnumerable<Operation> operations, CancellationToken cancellationToken)
    {
        foreach (var operation in operations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var disabled = variableService.ReadVariableValue(operation.Enabled).BoolValue == false || string.Equals(variableService.ReadVariableValue(operation.Enabled).TextValue, "false", StringComparison.OrdinalIgnoreCase);
            if (disabled || !conditionsService.ConditionsAreMet(operation.Conditions) ||
                cancellationToken.IsCancellationRequested) continue;
            var resolvedOperation = operationParameterResolver.PrepareOperationParameters(operation);
            outputService.Trace($"Executing operation: {resolvedOperation.Name}");
            await resolvedOperation.Run(cancellationToken);
        }
    }

    public Task ExecuteOperations(IEnumerable<DynamicValue> operations, CancellationToken cancellationToken)
    {
        List<Operation> operationList = [];
        foreach (var operation in operations)
            operationList.AddRange(operationFactory.GetOperations(operation));
        return ExecuteOperations(operationList, cancellationToken);
    }
}