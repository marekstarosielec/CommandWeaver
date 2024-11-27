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
    /// <param name="serializedCommand">The serialized JSON representation of the command.</param>
    void StoreCommandMetadata(
        string repositoryElementId,
        Command command,
        string serializedCommand);
}

/// <inheritdoc />
public class CommandMetadataService(
    IVariableService variableService,
    IJsonSerializer serializer,
    IFlowService flowService,
    IOutputService outputService) : ICommandMetadataService
{
    /// <inheritdoc />
    public void StoreCommandMetadata(
        string repositoryElementId,
        Command command,
        string serializedCommand)
    {
        outputService.Trace($"Storing metadata for command: {command.Name}");

        // Deserialize the serialized command as DynamicValue
        if (!serializer.TryDeserialize(serializedCommand, out DynamicValue? dynamicCommandDefinition, out Exception? exception))
        {
            flowService.NonFatalException(exception);
            outputService.Warning($"Failed to deserialize command metadata for {command.Name}");
            return;
        }

        // Prepare metadata for the command
        var commandInformation = new Dictionary<string, DynamicValue?>
        {
            ["key"] = new DynamicValue(command.Name),
            ["json"] = new DynamicValue(serializedCommand, true),
            ["id"] = new DynamicValue(repositoryElementId),
            ["definition"] = dynamicCommandDefinition! with { NoResolving = true }
        };

        // Store the metadata into variables
        var variableKey = $"commands[{command.Name}]";
        variableService.WriteVariableValue(
            VariableScope.Command,
            variableKey,
            new DynamicValue(new DynamicValueObject(commandInformation))
        );

        outputService.Debug($"Metadata for command '{command.Name}' stored with key '{variableKey}'.");
    }
}
