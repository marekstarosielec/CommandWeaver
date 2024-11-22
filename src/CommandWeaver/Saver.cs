using System.Collections.Immutable;

public interface ISaver
{
    Task Execute(CancellationToken cancellationToken);
}

public class Saver(IVariableService variables, IRepositoryElementStorage repositoryElementStorage, ISerializerFactory serializerFactory, IFlowService flow, IRepository repository, IOutputService output) : ISaver
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        var serializer = serializerFactory.GetDefaultSerializer(out var format);
        var defaultFileName = $"variables.{format}";

        var originalRepositories = repositoryElementStorage.Get();
        var repositoriesWithUpdatedVariables = variables.GetRepositoryElementStorage().Get();
        foreach (var repositoryWithUpdatedVariables in repositoriesWithUpdatedVariables)
        {
            if (repositoryWithUpdatedVariables.Content == null)
            {
                flow.Terminate("Failed to get changes in variables");
                continue;
            }

            var name = repositoryWithUpdatedVariables.Id;
            if (name == string.Empty)
                name = defaultFileName;
            var originalReporitory = originalRepositories.FirstOrDefault(r => r.Id == name);
            RepositoryElementContent? content = null;
            if (originalReporitory?.Content != null)
                content = originalReporitory.Content with { Variables = repositoryWithUpdatedVariables.Content.Variables?.Select(v => v with { RepositoryElementId = null}).ToImmutableList() };
            else if (originalReporitory == null)
                content = repositoryWithUpdatedVariables.Content with { Variables = repositoryWithUpdatedVariables.Content.Variables?.Select(v => v with { RepositoryElementId = null }).ToImmutableList() };
            else
            {
                //Avoid overwriting file if its contents could not be read - this could lead to loosing previous data.
                output.Warning($"Skipping saving to {name} as its content was not loaded.");
                continue;
            }
            if (!serializer.TrySerialize(content, out var serializedContent, out var exception))
            {
                flow.Terminate($"Failed to serialize variables");
                continue;
            }

            await repository.SaveRepositoryElement(name, serializedContent!, cancellationToken);
            continue;
        }
    }
}