using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Models;
using Models.Interfaces;
using Models.Interfaces.Context;

namespace Cli2Context;

public class ContextVariables : IContextVariables
{
    internal readonly IVariableValueCrawler variableValueCrawler = new VariableValueCrawler();
    internal readonly IVariableExtractionService variableExtractionService = new VariableExtractionService();
    internal readonly List<Variable> _builtIn = new();
    internal readonly List<Variable> _local = new();
    internal readonly List<Variable> _session = new();
    internal readonly List<Variable> _changes = new();

    public string CurrentSessionName
    {
        get => GetValueAsString("currentSessionName") ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => GetValueAsString("currentlyProcessedElement") as string;
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

    /// <summary>
    /// Returns value of variable from given key. Key can contain variables.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>

    public string? GetValueAsString(object? key)
    {
        if (key is not string stringKey)
            return null;

        if (string.IsNullOrEmpty(stringKey)) return null;
        var result = GetValue(stringKey);
        if (result == null) return null;
        if (result as Dictionary<string, object?> != null)
        {
            //Cannot return object as string;
            return null;
        }
        if (result as List<Dictionary<string, object?>> != null)
        {
            //Cannot return list as string;
            return null;
        }
        if (result is string stringResult)
            return stringResult;
        
        throw new InvalidOperationException("What type is that?");
    }

    private object? GetValue(string key)
    {
        // Check if the input is null, empty, or contains only whitespace
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = key;
        var variableName = variableExtractionService.ExtractVariableBetweenDelimiters(resolvedKey);
        while (variableName != null)
        {
            var evaluatedValue = EvaluateValue(variableName);
            string? stringEvaluatedValue = null;
            if (evaluatedValue == null)
            {
                //Log null value
            }
            else if (evaluatedValue is not string)
            {
                //Log non string value value
            }
            else stringEvaluatedValue = evaluatedValue as string;
            resolvedKey = variableExtractionService.ReplaceVariableInString(resolvedKey, variableName, stringEvaluatedValue);
            variableName = variableExtractionService.ExtractVariableBetweenDelimiters(resolvedKey);
        }
        return resolvedKey;
    }

    private object? EvaluateValue(string key)
    { 
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