/// <inheritdoc />
public class CommandWeaver(ILoader loader, ICommandService commandService, IFlowService flow, ISaver saver) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            flow.Terminate($"Command not provided.");
            return;
        }
        
        await loader.Execute(cancellationToken);
        commandService.Validate();
        
        var commandToExecute = commandService.Get(commandName);
        if (commandToExecute == null)
        {
            flow.Terminate($"Unknown command {commandName}");
            return;
        }
        commandService.PrepareCommandParameters(commandToExecute, arguments);
        await commandService.ExecuteOperations(commandToExecute.Operations, cancellationToken);
        await saver.Execute(cancellationToken);
    }
}

