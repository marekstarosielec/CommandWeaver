using Models;
using Models.Interfaces.Context;
using System.Collections.Immutable;

namespace Cli2Context;

public class ContextVariables : IContextVariables
{
    private readonly IOutput _output;
    private readonly ContextVariableStorage _variableStorage;
    private readonly ContextVariableResolver _variableResolver;

    public ContextVariables(IOutput output) : this(output, new ContextVariableStorage()) { }

    internal ContextVariables(IOutput output, ContextVariableStorage variableStorage)
    {
        _output = output;
        _variableStorage = variableStorage ?? new ContextVariableStorage();
        _variableResolver = new ContextVariableResolver(output, _variableStorage);
    }

    public string CurrentSessionName
    {
        get => _variableResolver.ResolveVariableValue(new VariableValue("currentSessionName"), true)?.TextValue ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", new VariableValue(value));
    }

    public string? CurrentlyProcessedElement
    {
        get => _variableResolver.ResolveVariableValue(new VariableValue("currentlyProcessedElement"), true)?.TextValue;
        set => SetVariableValue(VariableScope.Command, "currentlyProcessedElement", new VariableValue(value));
    }

    public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
    => _variableResolver.ResolveVariableValue(variableValue, treatTextValueAsVariable);

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

    public void SetVariableValue(VariableScope scope, string variableName, VariableValue value, string? description = null)
    {
        var topLevel = GetTopLevel(variableName);

        var existingVariable = _variableStorage.Changes.FirstOrDefault(v =>
            v.Key.Equals(topLevel, StringComparison.InvariantCultureIgnoreCase) && v.Scope == scope);
        
        if (variableName != topLevel && existingVariable == null) // new element in list
        {
            if (value.ObjectValue != null || value.ListValue != null)
                //Add single element or whole list
                _variableStorage.Changes.Add(new Variable { Key = topLevel, Value = value, Scope = scope, Description = description });
            else
                _output.Error("Tried to insert wrong type into list");
        }
        else if (variableName == topLevel && existingVariable == null) // new variable
            _variableStorage.Changes.Add(new Variable { Key = variableName, Value = value, Scope = scope, Description = description });
        else if (existingVariable != null) // existing variable
            existingVariable.Value = value;
    }


    /// <summary>
    /// Indicates if key points to top level of variable value (whole variable value).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    // private bool KeyIsTopLevel(string key) => !key.Contains(".") && !key.Contains("[");

    /// <summary>
    /// Returns variable name part of key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetTopLevel(string key) => key.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries).First();
}
