using System.Collections.Immutable;

public class Variables(IReader reader, IWriter writer, Storage storage) : IVariables
{
    public string CurrentSessionName
    {
        get => reader.ReadVariableValue(new DynamicValue("currentSessionName"), true)?.TextValue ?? "session1";
        set => WriteVariableValue(VariableScope.Application, "currentSessionName", new DynamicValue(value));
    }

    public string? CurrentlyLoadRepository
    {
        get => reader.ReadVariableValue(new DynamicValue("currentlyLoadRepository"), true)?.TextValue;
        set => WriteVariableValue(VariableScope.Command, "currentlyLoadRepository", new DynamicValue(value));
    }

    public string? CurrentlyLoadRepositoryElement
    {
        get => reader.ReadVariableValue(new DynamicValue("currentlyLoadRepositoryElement"), true)?.TextValue;
        set => WriteVariableValue(VariableScope.Command, "currentlyLoadRepositoryElement", new DynamicValue(value));
    }

    public Variable? FindVariable(string variableName) 
            => storage.Command.FirstOrDefault(v => v.Key == variableName)
            ?? storage.Session.FirstOrDefault(v => v.Key == variableName)
            ?? storage.Application.FirstOrDefault(v => v.Key == variableName)
            ?? storage.BuiltIn.FirstOrDefault(v => v.Key == variableName);

    public DynamicValue ReadVariableValue(DynamicValue variableValue, bool treatTextValueAsVariable = false)
    => reader.ReadVariableValue(variableValue, treatTextValueAsVariable);

    public void Add(RepositoryLocation repositoryLocation, IEnumerable<Variable> variables, string repositoryElementId)
    {
        //Add repository element id, it will be useful to save changes back.
        var elementsToImport = variables.Select(v => v with { RepositoryElementId = repositoryElementId });

        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                storage.BuiltIn = storage.BuiltIn.AddRange(elementsToImport).ToImmutableList();
                break;
            case RepositoryLocation.Application:
                storage.Application.AddRange(elementsToImport);
                break;
            case RepositoryLocation.Session:
                storage.Session.AddRange(elementsToImport);
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    public RepositoryElementStorage GetRepositoryElementStorage()
    {
        var result = new RepositoryElementStorage();
        var repositoryElements = storage.Session.GroupBy(v => v?.RepositoryElementId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var repositoryElement in repositoryElements)
            result.Add(new RepositoryElement(RepositoryLocation.Session, repositoryElement.Key, new RepositoryElementContent { Variables = repositoryElement.Value.ToImmutableList() }));
        repositoryElements = storage.Application.GroupBy(v => v?.RepositoryElementId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var repositoryElement in repositoryElements)
            result.Add(new RepositoryElement(RepositoryLocation.Application, repositoryElement.Key, new RepositoryElementContent { Variables = repositoryElement.Value.ToImmutableList() }));
        return result;

    }

    public void WriteVariableValue(VariableScope scope, string path, DynamicValue value, string? respositoryElementId = null) => writer.WriteVariableValue(scope, path, value, respositoryElementId);
}
