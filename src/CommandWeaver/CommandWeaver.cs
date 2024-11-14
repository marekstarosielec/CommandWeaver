/// <inheritdoc />
public class CommandWeaver(ILoader loader, ICommands commands, IFlow flow, ISaver saver) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commmandName))
        {
            flow.Terminate($"Command not provided.");
            return;
        }
        
        await loader.Execute(cancellationToken);
        commands.Validate();
        
        var commandToExecute = commands.Get(commmandName);
        if (commandToExecute == null)
        {
            flow.Terminate($"Unknown command {commmandName}");
            return;
        }
        commands.PrepareCommandParameters(commandToExecute, arguments);
        await commands.ExecuteCommand(commandToExecute, cancellationToken);
        await saver.Execute(cancellationToken);
    }

    
}

