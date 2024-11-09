using System.Collections.Immutable;

namespace Cli2Context;

public class ContextVariables : IContextVariables
{
    private readonly IContext _context;
    private readonly ContextVariableStorage _variableStorage;
    private readonly ContextVariableReader _variableReader;
    private readonly ContextVariableWriter _variableWriter;

    public ContextVariables(IContext context) : this(context, new ContextVariableStorage()) { }

    internal ContextVariables(IContext context, ContextVariableStorage variableStorage)
    {
        _context = context;
        _variableStorage = variableStorage ?? new ContextVariableStorage();
        _variableReader = new ContextVariableReader(context, _variableStorage);
        _variableWriter = new ContextVariableWriter(context, _variableStorage);
    }

    public string CurrentSessionName
    {
        get => _variableReader.ReadVariableValue(new DynamicValue("currentSessionName"), true)?.TextValue ?? "session1";
        set => WriteVariableValue(VariableScope.Application, "currentSessionName", new DynamicValue(value));
    }

    public string? CurrentlyProcessedElement
    {
        get => _variableReader.ReadVariableValue(new DynamicValue("currentlyProcessedElement"), true)?.TextValue;
        set => WriteVariableValue(VariableScope.Command, "currentlyProcessedElement", new DynamicValue(value));
    }

    public Variable? FindVariable(string variableName) 
            => _variableStorage.Command.FirstOrDefault(v => v.Key == variableName)
            ?? _variableStorage.Session.FirstOrDefault(v => v.Key == variableName)
            ?? _variableStorage.Application.FirstOrDefault(v => v.Key == variableName)
            ?? _variableStorage.BuiltIn.FirstOrDefault(v => v.Key == variableName);

    public DynamicValue ReadVariableValue(DynamicValue variableValue, bool treatTextValueAsVariable = false)
    => _variableReader.ReadVariableValue(variableValue, treatTextValueAsVariable);

    public void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent, string locationId)
    {
        var elementsToImport = elementsWithContent.Where(v => v != null).Cast<Variable>();

        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                _variableStorage.BuiltIn = _variableStorage.BuiltIn.AddRange(elementsToImport.Select(element => element with { LocationId = locationId }).ToImmutableList());
                break;
            case RepositoryLocation.Local:
                _variableStorage.Application.AddRange(elementsToImport.Select(element => element with { LocationId = locationId }).ToList());
                break;
            case RepositoryLocation.Session:
                _variableStorage.Session.AddRange(elementsToImport.Select(element => element with { LocationId = locationId }).ToList());
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    public Dictionary<string, List<Variable>> GetVariableList(RepositoryLocation repositoryLocation) => repositoryLocation switch
    {
        RepositoryLocation.Session => _variableStorage.Session.GroupBy(v => v?.LocationId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList()),
        RepositoryLocation.Local => _variableStorage.Application.GroupBy(v => v?.LocationId ?? string.Empty).ToDictionary(g => g.Key, g => g.ToList()),
        _ => throw new InvalidOperationException($"Cannot GetVariableList for RepositoryLocation=={repositoryLocation}")
    };

    public void WriteVariableValue(VariableScope scope, string path, DynamicValue value) => _variableWriter.WriteVariableValue(scope, path, value, "");
}
