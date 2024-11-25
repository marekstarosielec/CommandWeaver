using System.Collections.Immutable;

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
    ICommandService commandService,
    IJsonSerializer serializer,
    IFlowService flowService,
    IRepositoryElementStorage repositoryElementStorage) : ILoader
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        variableService.CurrentlyLoadRepository = "built-in";
        var elements = embeddedRepository.GetList(cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.BuiltIn, elements);
        outputSettings.SetStyles(variableService.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variableService.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Application);
        elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Application, elements);
        outputSettings.SetStyles(variableService.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variableService.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Session, variableService.CurrentSessionName);
        elements = repository.GetList(RepositoryLocation.Session, variableService.CurrentSessionName, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Session, elements);
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
    /// <param name="repositoryElementsSerialized"></param>
    /// <returns></returns>
    private async Task LoadRepositoryElements(RepositoryLocation repositoryLocation, IAsyncEnumerable<RepositoryElementSerialized> repositoryElementsSerialized)
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
            AddCommands(repositoryElementSerialized, repositoryContent.Commands);
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
    private void AddCommands(RepositoryElementSerialized repositoryElementSerialized, ImmutableList<Command?>? commands)
    {
        if (commands == null)
            return;
        
        var allCommands = commands.Where(c => c != null).Cast<Command>().ToList();
        commandService.Add(repositoryElementSerialized.Id, repositoryElementSerialized.Content!, allCommands);
    }
}

