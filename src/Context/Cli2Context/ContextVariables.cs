using System.Text.RegularExpressions;
using Models;
using Models.Interfaces.Context;

namespace Cli2Context;

public class ContextVariables : IContextVariables
{
    internal readonly List<Variable> _builtIn = new();
    internal readonly List<Variable> _local = new();
    internal readonly List<Variable> _session = new();
    internal readonly List<Variable> _changes = new();

    public string CurrentSessionName
    {
        get => GetVariableValue("currentSessionName")?.Value as string ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => GetVariableValue("currentlyProcessedElement")?.Value as string;
        set => SetVariableValue(VariableScope.Command, "currentlyProcessedElement", value);
    }
    
    public void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent)
    {
        var elementsToImport = elementsWithContent.Where(v => v != null).Cast<Variable>();
        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                _builtIn.Clear();
                foreach (var elementWithContentKey in elementsToImport)
                    _builtIn.Add(elementWithContentKey with { Scope = VariableScope.Application});
                break;
            case RepositoryLocation.Local:
                _local.Clear();
                foreach (var elementWithContentKey in elementsToImport)
                    _local.Add(elementWithContentKey with { Scope = VariableScope.Application});
                break;
            case RepositoryLocation.Session:
                _session.Clear();
                foreach (var elementWithContentKey in elementsToImport)
                    _session.Add(elementWithContentKey with { Scope = VariableScope.Session});
                break;
            //TODO: should fail?
            //default:
            //    break;
        }
    }

    record Section(string Name, string Type);
    public Variable? GetVariable(string key)
    {
        var pattern = @"([a-zA-Z0-9_-]+)|\[(.*?)\]";
        var matches = Regex.Matches(key, pattern);

        var result = new List<Section>();
        foreach (Match match in matches)
            if (match.Groups[1].Success)
                result.Add(new Section(match.Groups[1].Value, "property"));
            else if (match.Groups[2].Success)
                result.Add(new Section(match.Groups[2].Value, "index"));

        var variable = GetVariableValue(result[0].Name);
        object? currentElement = variable?.Value;
        for (var index = 1; index < result.Count; index++)
        {
            var section = result[index];
            if (section.Type == "index")
            {
                //throw if current element is not list
                var list = currentElement as List<object>;
                //compare by value or by key property if it is object
                foreach (var listElement in list)
                {
                    var element = (listElement as Dictionary<string, object?>)["key"];
                    if ((element as string)?.ToLower() == section.Name.ToLower())
                    {
                        currentElement = listElement;
                        break;
                    }
                }
            }
        }
        return null;
        //throw new NotImplementedException();
    }
    
    public Variable? GetVariableValue(string key) =>
        _changes.FirstOrDefault(v => v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && v.Scope == VariableScope.Command)
        ?? _changes.FirstOrDefault(v => v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && v.Scope == VariableScope.Session)
        ?? _changes.FirstOrDefault(v => v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && v.Scope == VariableScope.Application)
        ?? _session.FirstOrDefault(v => v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)) 
        ?? _local.FirstOrDefault(v => v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)) 
        ?? _builtIn.FirstOrDefault(v => v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)) 
        ?? null;
    
    public void SetVariableValue(VariableScope scope, string key, object? value, string? description = null)
    {
        //Add support for lists and complex objects.
        var existingVariable = _changes.FirstOrDefault(v =>
            v.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase) && v.Scope == scope);
        if (existingVariable != null)
            existingVariable.Value = value;
        else
            _changes.Add(new Variable { Key = key, Value = value, Scope = scope, Description = description});
    }
}