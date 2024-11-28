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
    public void Add(string repositoryElementId, string content, IEnumerable<Command> commands)
    {
        var commandList = commands.ToList();
        _commands.AddRange(commandList);

        // Add information about command into variables, so that they can be part of commands.
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement.GetProperty("commands");
        if (root.ValueKind != JsonValueKind.Array) return;

        for (var x = 0; x < commandList.Count; x++)
            commandMetadataService.StoreCommandMetadata(repositoryElementId, commandList[x], root[x].GetRawText());
    }

    /// <inheritdoc />
    public Command? Get(string name) =>
        _commands.FirstOrDefault(c => c.Name == name || c.OtherNames?.Any(n => n == name) == true);
    
    public async Task ExecuteOperations(IEnumerable<Operation> operations, CancellationToken cancellationToken)
    {
        foreach (var operation in operations)
            if (conditionsService.ConditionsAreMet(operation.Conditions) && !cancellationToken.IsCancellationRequested)
            {
                var resolvedOperation = operationParameterResolver.PrepareOperationParameters(operation);
                outputService.Trace($"Executing operation: {resolvedOperation.Name}");
                await resolvedOperation.Run(cancellationToken);
            }
    }
}