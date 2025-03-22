/// <summary>
/// Service responsible for processing and storing command metadata into variables.
/// </summary>
public interface ICommandMetadataService
{
    /// <summary>
    /// Processes and stores metadata for a command.
    /// </summary>
    /// <param name="repositoryElementId">The ID of the repository element containing the command.</param>
    /// <param name="command">The command to process.</param>
    void StoreCommandMetadata(
        string repositoryElementId,
        Command command);
}

/// <inheritdoc />
public class CommandMetadataService(
    IVariableService variableService,
    IOutputService outputService,
    ICommandParameterResolver commandParameterResolver) : ICommandMetadataService
{
    /// <inheritdoc />
    public void StoreCommandMetadata(
        string repositoryElementId,
        Command command)
    {
        outputService.Trace($"Storing metadata for command: {command.Name}");

        var mainName = command.GetAllNames().First();
        var resolvedParameters = commandParameterResolver.GetCommandParameters(command).Select(commandParameterResolver.GetCommandParameterAsDynamicValue).ToList();

        // Prepare metadata for the command
        var commandInformation = new Dictionary<string, DynamicValue?>
        {
            ["key"] = new (mainName),
            ["source"] = new (command.Source, true),
            ["id"] = new (repositoryElementId),
            ["parameters"] = new (new DynamicValueList( resolvedParameters)),
            ["definition"] = command.Definition with { NoResolving = true },
        };

        // Store the metadata into variables
        var variableKey = $"commands[{mainName}]";
        variableService.WriteVariableValue(
            VariableScope.Command,
            variableKey,
            new DynamicValue(new DynamicValueObject(commandInformation))
        );

        outputService.Trace($"Metadata for command '{mainName}' stored with key '{variableKey}'.");
    }
}
