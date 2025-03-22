using System.Collections.Immutable;

/// <summary>
/// Service responsible for loading variables and commands from repositories (e.g. from files).
/// </summary>
public interface ILoader
{
    /// <summary>
    /// Load variables and commands from repositories.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    Task Execute(CancellationToken cancellationToken);
}

/// <inheritdoc />
public class Loader(
    IVariableService variableService, 
    IEmbeddedRepository embeddedRepository, 
    IRepository repository,
    IOutputService outputService, 
    IOutputSettings outputSettings,
    ICommandService commandService,
    IJsonSerializer serializer,
    IRepositoryElementStorage repositoryElementStorage,
    IResourceService resourceService) : ILoader
{
    private const string BuiltInRepositoryName = "built-in";
    private const string StylesKey = "{{ styles }}";

    /// <inheritdoc />
    public async Task Execute(CancellationToken cancellationToken)
    {
        outputService.Trace("Execution started: Loading variables and commands from repositories.");
        await LoadBuiltInRepository(cancellationToken);
        await LoadApplicationRepository(cancellationToken);
        await LoadSessionRepository(cancellationToken);
        SetCommonVariables();
        outputSettings.Serializer = serializer;
        outputService.Trace("Execution completed: Variables and commands successfully loaded.");
    }

    /// <summary>
    /// Loads commands and variables from the built-in repository.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task LoadBuiltInRepository(CancellationToken cancellationToken)
    {
        outputService.Trace($"Loading built-in repository: {BuiltInRepositoryName}");
        variableService.CurrentlyLoadRepository = BuiltInRepositoryName;
        var elements = embeddedRepository.GetList(cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.BuiltIn, elements, cancellationToken);
        variableService.CurrentlyLoadRepository = null;
        ApplyStyles();
    }

    /// <summary>
    /// Loads commands and variables from the application repository.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task LoadApplicationRepository(CancellationToken cancellationToken)
    {
        outputService.Trace("Loading application repository.");
        variableService.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Application);
        var elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Application, elements, cancellationToken);
        variableService.CurrentlyLoadRepository = null;
        ApplyStyles();
    }

    /// <summary>
    /// Loads commands and variables from the session repository.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task LoadSessionRepository(CancellationToken cancellationToken)
    {
        outputService.Trace("Loading session repository.");
        variableService.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Session, variableService.CurrentSessionName);
        var elements = repository.GetList(RepositoryLocation.Session, variableService.CurrentSessionName, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Session, elements, cancellationToken);
        variableService.CurrentlyLoadRepository = null;
        ApplyStyles();
    }

    /// <summary>
    /// Applies styles based on the variables read from the repository.
    /// </summary>
    private void ApplyStyles()
    {
        var styles = variableService.ReadVariableValue(new DynamicValue(StylesKey));
        outputService.Trace("Applying styles from variables.");
        outputSettings.SetStyles(styles);
    }

    /// <summary>
    /// Sets common variables for the application.
    /// </summary>
    private void SetCommonVariables()
    {
        outputService.Trace("Setting common variables.");
        variableService.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(repository.GetPath(RepositoryLocation.Application)));
        variableService.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(repository.GetPath(RepositoryLocation.Session, variableService.CurrentSessionName)));
    }

    /// <summary>
    /// Loads commands and variables from repository elements.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository (e.g., built-in, application, session).</param>
    /// <param name="repositoryElementsInformation">The serialized repository elements to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task LoadRepositoryElements(RepositoryLocation repositoryLocation, IAsyncEnumerable<RepositoryElementInformation> repositoryElementsInformation, CancellationToken cancellationToken)
    {
        await foreach (var repositoryElementInformation in repositoryElementsInformation.WithCancellation(cancellationToken))
        {
            variableService.CurrentlyLoadRepositoryElement = repositoryElementInformation.FriendlyName;
            outputService.Trace($"Processing element {variableService.CurrentlyLoadRepository}\\{variableService.CurrentlyLoadRepositoryElement}");

            resourceService.Add(repositoryElementInformation);
            if (!string.Equals(JsonHelper.Extension, repositoryElementInformation.Format, StringComparison.OrdinalIgnoreCase))
            {
                outputService.Trace($"Skipped element {variableService.CurrentlyLoadRepositoryElement} due to unsupported format: {repositoryElementInformation.Format}");
                continue;
            }
            
            var content = repositoryElementInformation.ContentAsString?.Value;
            if (string.IsNullOrWhiteSpace(content))
            {
                outputService.Warning($"Element '{variableService.CurrentlyLoadRepositoryElement}' is empty in repository '{repositoryLocation}'");
                continue;
            }

            if (!serializer.TryDeserialize(content, out RepositoryElementContent? repositoryContent, out var exception) || repositoryContent == null)
            {
                HandleDeserializationError(repositoryLocation, repositoryElementInformation, exception);
                continue;
            }

            repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementInformation.Id, repositoryContent));

            AddVariables(repositoryLocation, repositoryElementInformation.Id, repositoryContent.Variables);
            AddCommands(repositoryElementInformation, repositoryContent.Commands);
        }

        variableService.CurrentlyLoadRepositoryElement = null;
    }

    /// <summary>
    /// Adds variables from the repository to the variable service.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository.</param>
    /// <param name="repositoryElementId">The ID of the repository element.</param>
    /// <param name="variables">The variables to add.</param>
    private void AddVariables(RepositoryLocation repositoryLocation, string repositoryElementId, ImmutableList<Variable?>? variables)
    {
        if (variables == null)
            return;

        variableService.Add(repositoryLocation, repositoryElementId, variables.OfType<Variable>().ToList());
        outputService.Trace($"Added variables from repository location: {repositoryLocation}, element: {repositoryElementId}");
    }

    /// <summary>
    /// Adds commands from the repository to the command service.
    /// </summary>
    /// <param name="repositoryElementInformation">The serialized repository element containing commands.</param>
    /// <param name="commands">The list of commands to add.</param>
    private void AddCommands(RepositoryElementInformation repositoryElementInformation, ImmutableList<Command?>? commands)
    {
        if (commands == null)
            return;

        commandService.Add(repositoryElementInformation.Id, commands.OfType<Command>().ToList());
        outputService.Trace($"Added commands from repository element: {repositoryElementInformation.FriendlyName}");
    }

    /// <summary>
    /// Handles errors during deserialization of repository elements.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository.</param>
    /// <param name="element">The repository element that failed deserialization.</param>
    /// <param name="exception">The exception encountered during deserialization, if any.</param>
    private void HandleDeserializationError(RepositoryLocation repositoryLocation, RepositoryElementInformation element, Exception? exception)
    {
        repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, element.Id, null));
        if (exception!=null)
            outputService.WriteException(exception);
        outputService.Warning($"Failed to deserialize element '{variableService.CurrentlyLoadRepositoryElement}' in repository '{variableService.CurrentlyLoadRepository}'");
    }
}
