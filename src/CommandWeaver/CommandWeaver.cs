/// <inheritdoc />
public class CommandWeaver(ILoader loader, ICommandService iCommandService, IFlowService flow, ISaver saver) : ICommandWeaver
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
        iCommandService.Validate();
        
        var commandToExecute = iCommandService.Get(commandName);
        if (commandToExecute == null)
        {
            flow.Terminate($"Unknown command {commandName}");
            return;
        }
        iCommandService.PrepareCommandParameters(commandToExecute, arguments);
        await iCommandService.ExecuteOperations(commandToExecute.Operations, cancellationToken);
        await saver.Execute(cancellationToken);
    }

    
}

