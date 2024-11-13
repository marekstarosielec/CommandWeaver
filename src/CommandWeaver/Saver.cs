using System.Collections.Immutable;

public interface ISaver
{
    void Execute(CancellationToken cancellationToken);
}

public class Saver(IVariables variables, IRepositoryStorage repositoryStorage, ISerializerFactory serializerFactory, IFlow flow, IRepository repository) : ISaver
{
    public void Execute(CancellationToken cancellationToken)
    {
        var serializer = serializerFactory.GetDefaultSerializer(out var format);
        var defaultFileName = $"variables.{format}";

        var originalRepositories = repositoryStorage.Get();
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
            var sessionName = repositoryWithUpdatedVariables.repositoryLocation == RepositoryLocation.Session
                ? variables.CurrentSessionName
                : null;
            var originalReporitory = originalRepositories.FirstOrDefault(r => r.id == name);
            if (originalReporitory?.content != null)
            {
                var combinedContent = originalReporitory.content with { Variables = repositoryWithUpdatedVariables.content.Variables?.Select(v => v with { RepositoryElementId = null}).ToImmutableList() };
                if (!serializer.TrySerialize(combinedContent, out var serializedContent, out var exception))
                {
                    flow.Terminate($"Failed to serialize variables");
                    continue;
                }
                repository.SaveList(repositoryWithUpdatedVariables.repositoryLocation, name, sessionName, serializedContent!, cancellationToken);
                continue;
            }
            else if (originalReporitory == null)
            {
                //There was no file with that name, so we create new with changed values only.
                if (!serializer.TrySerialize(repositoryWithUpdatedVariables.content, out var serializedContent, out var exception))
                {
                    flow.Terminate($"Failed to serialize variables");
                    continue;
                }
                repository.SaveList(repositoryWithUpdatedVariables.repositoryLocation, name, sessionName, serializedContent!, cancellationToken);
                continue;
            }
        }
    }

    //private ImmutableList<Variable>? CombineVariables(ImmutableList<Variable>? originalVariables, ImmutableList<Variable>? changedVariables)
    //{
    //    if (originalVariables == null)
    //        return changedVariables;
    //    if (changedVariables == null)
    //        return originalVariables;
        
    //    var result = originalVariables.ToList();
    //    foreach (var changedVariable in changedVariables)
    //    {
    //        var originalVariable = result.FirstOrDefault(v => v.Key == changedVariable.Key);
    //        if (originalVariable != null)
    //            result
    //    }
    //}
}

