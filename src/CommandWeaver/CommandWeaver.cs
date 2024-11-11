/// <inheritdoc />
public class CommandWeaver(ILoader loader, ICommands commands, IOutput output, IFlow flow) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        //variables.WriteVariableValue(VariableScope.Command, "BuiltInPath", new DynamicValue(_repository.GetPath(RepositoryLocation.BuiltIn)));
        //variables.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Application)));
        //variables.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Session, variables.CurrentSessionName)));

        await loader.Execute(cancellationToken);
        commands.Validate();
        
        if (string.IsNullOrWhiteSpace(commmandName))
        {
            flow.Terminate($"Command not provided.");
            return;
        }
        var commandToExecute = commands.Get(commmandName);
        if (commandToExecute == null)
        {
            flow.Terminate($"Unknown command {commmandName}");
            return;
        }
        commands.PrepareCommandParameters(commandToExecute, arguments);
         



    }

    
}

