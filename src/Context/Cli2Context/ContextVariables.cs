using Models;
using Models.Interfaces.Context;
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
        get => _variableReader.ReadVariableValue(new VariableValue("currentSessionName"), true)?.TextValue ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", new VariableValue(value));
    }

    public string? CurrentlyProcessedElement
    {
        get => _variableReader.ReadVariableValue(new VariableValue("currentlyProcessedElement"), true)?.TextValue;
        set => SetVariableValue(VariableScope.Command, "currentlyProcessedElement", new VariableValue(value));
    }

    public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
    => _variableReader.ReadVariableValue(variableValue, treatTextValueAsVariable);

    public void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent, string locationId)
    {
        var elementsToImport = elementsWithContent.Where(v => v != null).Cast<Variable>();

        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                _variableStorage.BuiltIn = elementsToImport.Select(element => element with { Scope = VariableScope.Application, LocationId = locationId }).ToImmutableList();
                break;
            case RepositoryLocation.Local:
                _variableStorage.Local = elementsToImport.Select(element => element with { Scope = VariableScope.Application, LocationId = locationId }).ToImmutableList();
                break;
            case RepositoryLocation.Session:
                _variableStorage.Session = elementsToImport.Select(element => element with { Scope = VariableScope.Session, LocationId = locationId }).ToImmutableList();
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    public void SetVariableValue(VariableScope scope, string path, VariableValue value, string? description = null)
    {
        
        var variableName = VariableValuePath.GetVariableName(path);

        var existingVariable = _variableStorage.Changes.FirstOrDefault(v =>
            v.Key.Equals(variableName) && v.Scope == scope);

        var newValue = _variableReader.ReadVariableValue(value);

        if (path != variableName && existingVariable == null) // new element in list
        {
            if (value.ObjectValue != null || value.ListValue != null)
                //Add single element or whole list
                _variableStorage.Changes.Add(new Variable { Key = variableName, Value = value, Scope = scope, Description = description });
            else
                _context.Services.Output.Error("Tried to insert wrong type into list");
        }
        else if (path == variableName && existingVariable == null) // new variable
            _variableStorage.Changes.Add(new Variable { Key = path, Value = value, Scope = scope, Description = description });
        else if (existingVariable != null) // existing variable
            existingVariable.Value = value;
    }
}
