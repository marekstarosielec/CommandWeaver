using System.Collections.Immutable;

public interface ISaver
{
    Task Execute(CancellationToken cancellationToken);
}

public class Saver(IVariables variables, IRepositoryElementStorage repositoryElementStorage, ISerializerFactory serializerFactory, IFlow flow, IRepository repository, IOutput output) : ISaver
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        var serializer = serializerFactory.GetDefaultSerializer(out var format);
        var defaultFileName = $"variables.{format}";

        var originalRepositories = repositoryElementStorage.Get();
        var repositoriesWithUpdatedVariables = variables.GetRepositoryElementStorage().Get();
        foreach (var repositoryWithUpdatedVariables in repositoriesWithUpdatedVariables)
        {
            if (repositoryWithUpdatedVariables.content == null)
            {
                flow.Terminate("Failed to get changes in variables");
                continue;
            }

            var name = repositoryWithUpdatedVariables.id;
            if (name == string.Empty)
                name = defaultFileName;
            var originalReporitory = originalRepositories.FirstOrDefault(r => r.id == name);
            RepositoryElementContent? content = null;
            if (originalReporitory?.content != null)
                content = originalReporitory.content with { Variables = repositoryWithUpdatedVariables.content.Variables?.Select(v => v with { RepositoryElementId = null}).ToImmutableList() };
            else if (originalReporitory == null)
                content = repositoryWithUpdatedVariables.content with { Variables = repositoryWithUpdatedVariables.content.Variables?.Select(v => v with { RepositoryElementId = null }).ToImmutableList() };
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

            await repository.SaveList(name, serializedContent!, cancellationToken);
            continue;
        }
    }
}