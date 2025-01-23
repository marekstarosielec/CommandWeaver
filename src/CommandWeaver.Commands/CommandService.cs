using System.Text.Json;

/// <inheritdoc />
public class CommandService(
    IConditionsService conditionsService,
    ICommandMetadataService commandMetadataService,
    IOperationParameterResolver operationParameterResolver,
    IOutputService outputService) : ICommandService
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
        _commands.FirstOrDefault(c => c.Name == name || c.OtherNames?.Any(n => n == name) == true);
    
    public async Task ExecuteOperations(IEnumerable<Operation> operations, CancellationToken cancellationToken)
    {
        foreach (var operation in operations)
            if (operation.Enabled && conditionsService.ConditionsAreMet(operation.Conditions) && !cancellationToken.IsCancellationRequested)
            {
                var resolvedOperation = operationParameterResolver.PrepareOperationParameters(operation);
                outputService.Trace($"Executing operation: {resolvedOperation.Name}");
                await resolvedOperation.Run(cancellationToken);
            }
    }
}