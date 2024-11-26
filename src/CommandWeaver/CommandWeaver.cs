/// <inheritdoc />
public class CommandWeaver(ICommandService commandService, IFlowService flowService, ILoader loader, ISaver saver, IOutputService outputService) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            flowService.Terminate($"Command not provided.");
            return;
        }
        
        outputService.Trace($"Starting execution for command: {commandName}");
        
        await loader.Execute(cancellationToken);
        commandService.Validate();
        
        var commandToExecute = commandService.Get(commandName);
        if (commandToExecute == null)
        {
            flowService.Terminate($"Unknown command {commandName}");
            return;
        }
        commandService.PrepareCommandParameters(commandToExecute, arguments);
        await commandService.ExecuteOperations(commandToExecute.Operations, cancellationToken);
        await saver.Execute(cancellationToken);
        
        outputService.Trace($"Execution completed for command: {commandName}");
    }
}

