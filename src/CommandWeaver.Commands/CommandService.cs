using System.Collections.Immutable;
using System.Text.Json;

/// <inheritdoc />
public class CommandService(
    IOutputService outputService,
    IFlowService flowService,
    IConditionsService conditionsService,
    IVariableService variableService,
    IRepositoryElementStorage repositoryElementStorage,
    ICommandMetadataService commandMetadataService,
    IOutputSettings outputSettings,
    IValidator validator,
    ICommandParameterResolver commandParameterResolver,
    IOperationParameterResolver operationParameterResolver) : ICommandService
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

    public Command? Get(string name) =>
        _commands.FirstOrDefault(c => c.Name == name || c.OtherNames?.Any(n => n == name) == true);

    public void PrepareCommandParameters(Command command, Dictionary<string, string> arguments)
    {
        // Convert command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in command.Parameters.Union(BuiltInCommandParameters.List))
        {
            var resolvedValue = commandParameterResolver.ResolveArgument(parameter, arguments, flowService, outputService);
            variableService.WriteVariableValue(VariableScope.Command, parameter.Key, resolvedValue);
        }

        // Check if all required parameters have value
        foreach (var parameter in command.Parameters)
        {
            var variable = variableService.FindVariable(parameter.Key);
            var value = variable == null ? null : variableService.ReadVariableValue(variable.Value);
            if (parameter.Required && string.IsNullOrWhiteSpace(value?.TextValue))
            {
                flowService.Terminate($"Parameter {parameter.Key} requires value.");
                return;
            }
        }

        outputSettings.CurrentLogLevel = variableService.LogLevel;
    }

    public void Validate() => validator.ValidateCommands(repositoryElementStorage.Get());

    public async Task ExecuteOperations(List<Operation> operations, CancellationToken cancellationToken)
    {
        foreach (var operation in operations)
            if (!conditionsService.ConditionsAreMet(operation.Conditions) && !cancellationToken.IsCancellationRequested)
                await ExecuteOperation(operation, cancellationToken);
    }

    private async Task ExecuteOperation(Operation operation, CancellationToken cancellationToken) =>
        await operationParameterResolver.PrepareOperationParameters(operation).Run(cancellationToken);
}