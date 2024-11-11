/// <inheritdoc />
public class CommandWeaver(ILoader loader, ICommands commands, IFlow flow, IVariables variables) : ICommandWeaver
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

        ////Save changes in variables
        //var variableList = variables.GetVariableList(RepositoryLocation.Application);
        //foreach (var locationId in variableList.Keys)
        //{
        //    var resolvedLocationId = string.IsNullOrWhiteSpace(locationId) ? "variables.json" : locationId;
        //    var originalFile = _originalRepositories.ContainsKey(RepositoryLocation.Application) && _originalRepositories[RepositoryLocation.Application].ContainsKey(resolvedLocationId)
        //                ? _originalRepositories[RepositoryLocation.Application][resolvedLocationId] : null;
        //    if (originalFile != null)
        //        originalFile.Variables = variableList[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList();
        //    else
        //        originalFile = new RepositoryElementContent { Variables = variableList[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList() };

        //    var serializer = _serializerFactory.GetSerializer("json");
        //    if (serializer.TrySerialize(originalFile, out var content, out var exception))
        //        repository.SaveList(RepositoryLocation.Application, resolvedLocationId, null, content, cancellationToken);
        //}
    }

    
}

