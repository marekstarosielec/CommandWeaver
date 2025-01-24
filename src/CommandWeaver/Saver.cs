using System.Collections.Immutable;

/// <summary>
/// Service responsible for saving variables back to repositories (e.g., from files).
/// </summary>
public interface ISaver
{
    /// <summary>
    /// Saves variables to repositories.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task Execute(CancellationToken cancellationToken);
}

/// <inheritdoc />
public class Saver(
    IRepositoryElementStorage repositoryElementStorage,
    IVariableStorage variableStorage,
    IJsonSerializer serializer,
    IFlowService flow,
    IRepository repository,
    IOutputService output) : ISaver
{
    /// <inheritdoc />
    public async Task Execute(CancellationToken cancellationToken)
    {
        output.Debug("Starting the save process for modified repositories.");

        foreach (var repositoryWithUpdatedVariables in GetModifications())
            await SaveRepository(repositoryWithUpdatedVariables, cancellationToken);

        output.Debug("Save process completed.");
    }

    /// <summary>
    /// Saves a single repository element to its corresponding location.
    /// </summary>
    /// <param name="repositoryWithUpdatedVariables">The repository element with updated variables to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    private async Task SaveRepository(RepositoryElement repositoryWithUpdatedVariables, CancellationToken cancellationToken)
    {
        if (repositoryWithUpdatedVariables.Content == null)
        {
            flow.Terminate("Failed to get changes in variables");
            return;
        }

        var name = GetRepositoryName(repositoryWithUpdatedVariables.Id);

        output.Debug($"Preparing to save repository: {name}");

        var originalRepository = repositoryElementStorage.Get().FirstOrDefault(r => r.Id == name);

        var contentToSave = GetContentToSave(originalRepository, repositoryWithUpdatedVariables);
        if (contentToSave == null)
        {
            output.Warning($"Skipping saving to {name} as its content was not loaded.");
            return;
        }
        if (!serializer.TrySerialize(contentToSave, out var serializedContent, out var exception))
        {
            flow.FatalException(exception, "Failed to serialize variables");
            return;
        }

        await repository.SaveRepositoryElement(name, serializedContent!, cancellationToken);
        output.Debug($"Successfully saved repository: {name}");
    }

    /// <summary>
    /// Determines the content to save based on the original and updated repository elements.
    /// </summary>
    /// <param name="originalRepository">The original repository element before updates.</param>
    /// <param name="repositoryWithUpdatedVariables">The repository element with updated variables.</param>
    /// <returns>
    /// A <see cref="RepositoryElementContent"/> containing the variables to save, or <c>null</c> if saving should be skipped.
    /// </returns>
    private RepositoryElementContent? GetContentToSave(RepositoryElement? originalRepository, RepositoryElement repositoryWithUpdatedVariables)
    {
        var variables = repositoryWithUpdatedVariables.Content!.Variables?
            .Select(v => v != null ? v with { RepositoryElementId = null } : null).ToImmutableList();
        
        // For builtin repositories we do not rewrite commands.
        if (originalRepository?.RepositoryLocation == RepositoryLocation.BuiltIn) 
            return new RepositoryElementContent { Variables = variables };
        // For application and session repositories we rewrite everything, just replace variables with new values.
        if (originalRepository?.Content != null) 
            return originalRepository.Content with { Variables = variables };
        // For newly created repositories we just write varaibles.
        if (originalRepository == null)       
            return repositoryWithUpdatedVariables.Content! with { Variables = variables };
        return null;
    }

    /// <summary>
    /// Generates the name of the repository element to save.
    /// </summary>
    /// <param name="id">The ID of the repository element.</param>
    /// <returns>The repository name, defaulting to "variables.json" if the ID is null or empty.</returns>
    private string GetRepositoryName(string? id) =>
        string.IsNullOrWhiteSpace(id) ? $"variables.{JsonHelper.Extension}" : id;

    /// <summary>
    /// Groups and transforms variables by their repository element ID.
    /// </summary>
    /// <param name="source">The collection of variables to group.</param>
    /// <returns>A dictionary grouping variables by repository element ID.</returns>
    private static Dictionary<string, List<Variable>> GroupAndTransform(IEnumerable<Variable> source) =>
        source
            .GroupBy(v => v.RepositoryElementId ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.ToList());

    /// <summary>
    /// Identifies repository elements with modifications to save.
    /// </summary>
    /// <returns>An enumerable of <see cref="RepositoryElement"/> instances with updated variables.</returns>
    private IEnumerable<RepositoryElement> GetModifications()
    {
        output.Debug("Grouping variables for session and application repositories.");
        foreach (var repositoryElement in GroupAndTransform(variableStorage.Session))
            yield return new RepositoryElement(RepositoryLocation.Session, repositoryElement.Key, new RepositoryElementContent { Variables = repositoryElement.Value.Select(v => (Variable?)v).ToImmutableList() });
        foreach (var repositoryElement in GroupAndTransform(variableStorage.Application))
            yield return new RepositoryElement(RepositoryLocation.Application, repositoryElement.Key, new RepositoryElementContent { Variables = repositoryElement.Value.Select(v => (Variable?)v).ToImmutableList() });
    }
}
