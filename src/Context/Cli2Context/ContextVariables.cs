using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Models;
using Models.Interfaces;
using Models.Interfaces.Context;

namespace Cli2Context;

public class ContextVariables : IContextVariables
{
    internal readonly IVariableValueCrawler variableValueCrawler = new VariableValueCrawler();
    internal readonly List<Variable> _builtIn = new();
    internal readonly List<Variable> _local = new();
    internal readonly List<Variable> _session = new();
    internal readonly List<Variable> _changes = new();

    public string CurrentSessionName
    {
        get => GetVariableValue2("currentSessionName") as string ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => GetVariableValue2("currentlyProcessedElement") as string;
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

    public object? GetVariableValue2(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        //Resolve variables inside keys first - code in Variable.EvaluateVariables
        var pattern = @"\{\{\s*(.*?)\s*\}\}"; // Regex pattern to match {{ content }}
        var regex = new Regex(pattern);

        //TODO: this might need matching inner variables (variables inside variables) first.
        while (regex.IsMatch(key))
        {
            key = regex.Replace(key, match =>
            {
                // Extract the content between {{ and }}
                var key = match.Groups[1].Value;

                // Get the replacement from the provided method
                var replacement = GetVariableValue2(key) as string ?? string.Empty;
                return replacement;
            });
        }



        var builtIn = variableValueCrawler.GetSubValue(_builtIn, key);
        var local = variableValueCrawler.GetSubValue(_local, key);
        var session = variableValueCrawler.GetSubValue(_session, key);
        var changes = variableValueCrawler.GetSubValue(_changes, key);

        var isList = false;
        if (builtIn as List<Dictionary<string, object?>> != null 
            || local as List<Dictionary<string, object?>> != null
            || session as List<Dictionary<string, object?>> != null
            || changes as List<Dictionary<string, object?>> != null)
            isList = true;

        if (isList)
            if (builtIn as Dictionary<string, object?> != null
                || local as Dictionary<string, object?> != null
                || session as Dictionary<string, object?> != null
                || changes as Dictionary<string, object?> != null)
                {
                    //Fail = some elements contain lists, some don't - should not happen
                    return null;
                }
        
        if (!isList)
            return changes ?? session ?? local ?? builtIn; 
        else
        {
            //Return all rows
            var result = new List<ImmutableDictionary<string, object?>>();
            if (builtIn is List<Dictionary<string, object?>> builtInList)
                foreach (var item in builtInList)
                    result.Add(item.ToImmutableDictionary());

            if (local is List<Dictionary<string, object?>> localList)
                foreach (var item in localList)
                    if (!result.Any(r => (r["key"] as string).Equals(item["key"])))
                        result.Add(item.ToImmutableDictionary());

            return result.ToImmutableList();
        }
        throw new NotImplementedException();
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