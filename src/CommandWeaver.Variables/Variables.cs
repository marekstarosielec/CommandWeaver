using System.Collections.Immutable;

public class Variables(IReader reader, IWriter writer, Storage storage) : IVariables
{
    public string CurrentSessionName
    {
        get => reader.ReadVariableValue(new DynamicValue("currentSessionName"), true)?.TextValue ?? "session1";
        set => WriteVariableValue(VariableScope.Application, "currentSessionName", new DynamicValue(value));
    }

    public string? CurrentlyProcessedElement
    {
        get => reader.ReadVariableValue(new DynamicValue("currentlyProcessedElement"), true)?.TextValue;
        set => WriteVariableValue(VariableScope.Command, "currentlyProcessedElement", new DynamicValue(value));
    }

    public Variable? FindVariable(string variableName) 
            => storage.Command.FirstOrDefault(v => v.Key == variableName)
            ?? storage.Session.FirstOrDefault(v => v.Key == variableName)
            ?? storage.Application.FirstOrDefault(v => v.Key == variableName)
            ?? storage.BuiltIn.FirstOrDefault(v => v.Key == variableName);

    public DynamicValue ReadVariableValue(DynamicValue variableValue, bool treatTextValueAsVariable = false)
    => reader.ReadVariableValue(variableValue, treatTextValueAsVariable);

    public void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent, string locationId)
    {
        var elementsToImport = elementsWithContent.Where(v => v != null).Cast<Variable>();

        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                storage.BuiltIn = storage.BuiltIn.AddRange(elementsToImport.Select(element => element with { LocationId = locationId }).ToImmutableList());
                break;
            case RepositoryLocation.Application:
                storage.Application.AddRange(elementsToImport.Select(element => element with { LocationId = locationId }).ToList());
                break;
            case RepositoryLocation.Session:
                storage.Session.AddRange(elementsToImport.Select(element => element with { LocationId = locationId }).ToList());
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    public Dictionary<string, List<Variable>> GetVariableList(RepositoryLocation repositoryLocation) => repositoryLocation switch
    {
        RepositoryLocation.Session => storage.Session.GroupBy(v => v?.LocationId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList()),
        RepositoryLocation.Application => storage.Application.GroupBy(v => v?.LocationId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList()),
        _ => throw new InvalidOperationException($"Cannot GetVariableList for RepositoryLocation=={repositoryLocation}")
    };

    public void WriteVariableValue(VariableScope scope, string path, DynamicValue value) => writer.WriteVariableValue(scope, path, value, "");
}
