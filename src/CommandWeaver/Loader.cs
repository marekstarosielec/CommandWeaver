using System.Collections.Immutable;
using System.Text.Json;

/// <summary>
/// Service responsible for loading variables and commands from repository (e.g. from files).
/// </summary>
public interface ILoader
{
    /// <summary>
    /// Load variables and commands from repository.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(CancellationToken cancellationToken);
}

public class Loader(
    IVariableService variableService, 
    IEmbeddedRepository embeddedRepository, 
    IRepository repository,
    IOutputService outputService, 
    IOutputSettings outputSettings,
    ICommandService iCommandService,
    IJsonSerializer serializer,
    IFlowService flowService,
    IRepositoryElementStorage repositoryElementStorage) : ILoader
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        variableService.CurrentlyLoadRepository = "built-in";
        var elements = embeddedRepository.GetList(cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.BuiltIn, null, elements);
        outputSettings.SetStyles(variableService.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variableService.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Application);
        elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Application, null, elements);
        outputSettings.SetStyles(variableService.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variableService.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Session, variableService.CurrentSessionName);
        elements = repository.GetList(RepositoryLocation.Session, variableService.CurrentSessionName, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Session, variableService.CurrentSessionName, elements);
        variableService.CurrentlyLoadRepository = null;
        outputSettings.SetStyles(variableService.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variableService.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(repository.GetPath(RepositoryLocation.Application)));
        variableService.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(repository.GetPath(RepositoryLocation.Session, variableService.CurrentSessionName)));
        
        outputSettings.Serializer = serializer;
    }
    
    /// <summary>
    /// Loads commands and variables from repository.
    /// </summary>
    /// <param name="repositoryLocation"></param>
    /// <param name="sessionName"></param>
    /// <param name="repositoryElementsSerialized"></param>
    /// <returns></returns>
    private async Task LoadRepositoryElements(RepositoryLocation repositoryLocation, string? sessionName, IAsyncEnumerable<RepositoryElementSerialized> repositoryElementsSerialized)
    {
        await foreach (var repositoryElementSerialized in repositoryElementsSerialized)
        {
            variableService.CurrentlyLoadRepositoryElement = repositoryElementSerialized.FriendlyName;
            outputService.Debug($"Processing element {variableService.CurrentlyLoadRepository}\\{variableService.CurrentlyLoadRepositoryElement}");

            if (!string.Equals(serializer.Extension, repositoryElementSerialized.Format, StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrWhiteSpace(repositoryElementSerialized.Content))
            {
                outputService.Warning($"Element {variableService.CurrentlyLoadRepositoryElement} is empty");
                continue;
            }
            
            if (!serializer.TryDeserialize(repositoryElementSerialized.Content, out RepositoryElementContent? repositoryContent, out var exception) || repositoryContent == null)
            {
                //Still save information about repository, to avoid overriding it with partial content.
                repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementSerialized.Id, repositoryContent));
                
                flowService.NonFatalException(exception);
                
                outputService.Warning($"Element {variableService.CurrentlyLoadRepositoryElement} failed to deserialize");
                continue;
            }

            repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementSerialized.Id, repositoryContent));

            AddVariables(repositoryLocation, repositoryElementSerialized.Id, repositoryContent.Variables);
            AddCommands(repositoryContent.Commands, repositoryElementSerialized);
        }

        variableService.CurrentlyLoadRepositoryElement = null;
    }

    /// <summary>
    /// Make variables from repository accessible in CommandWeaver.
    /// </summary>
    /// <param name="repositoryLocation"></param>
    /// <param name="repositoryElementId"></param>
    /// <param name="variables"></param>
    void AddVariables(RepositoryLocation repositoryLocation, string repositoryElementId,
        ImmutableList<Variable?>? variables)
    {
        if (variables == null)
            return;
        
        var allVariables = variables.Where(c => c != null).Cast<Variable>().ToList();
        variableService.Add(repositoryLocation, repositoryElementId, allVariables);
    }
    
    /// <summary>
    /// Make commands from repository accessible in CommandWeaver.
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="repositoryElementSerialized"></param>
    private void AddCommands(ImmutableList<Command?>? commands, RepositoryElementSerialized repositoryElementSerialized)
    {
        if (commands == null)
            return;
        
        var allCommands = commands.Where(c => c != null).Cast<Command>().ToList();
        iCommandService.Add(allCommands);

        //Add information about command into variables, so that they can be part of commands.
        using var doc = JsonDocument.Parse(repositoryElementSerialized.Content!);
        var root = doc.RootElement.GetProperty("commands");
        if (root.ValueKind == JsonValueKind.Array)
        {
            for (var x = 0; x < allCommands.Count; x++)
            {
                var command = allCommands[x];
                var serializedCommand = root[x].GetRawText();
                
                //Deserialize command again, but as a DynamicValue - so it can be accessed from variables.
                if (!serializer.TryDeserialize(serializedCommand, out DynamicValue? dynamicCommandDefinition,
                        out Exception? exception))
                {
                    flowService.NonFatalException(exception);
                    outputService.Warning($"Element {variableService.CurrentlyLoadRepositoryElement} failed to deserialize");
                    continue;
                }
                
                //Prepare information about command.
                var commandInformation = new Dictionary<string, DynamicValue?>();
                commandInformation["key"] = new DynamicValue(command.Name);
                commandInformation["json"] = new DynamicValue(serializedCommand, true);
                commandInformation["id"] = new DynamicValue(repositoryElementSerialized.Id);
                commandInformation["definition"] = dynamicCommandDefinition! with { NoResolving = true };
              
                //Add command information to variables.
                variableService.WriteVariableValue(VariableScope.Command, $"commands[{command.Name}]", new DynamicValue(new DynamicValueObject(commandInformation)));
            }
        }
    }
}

