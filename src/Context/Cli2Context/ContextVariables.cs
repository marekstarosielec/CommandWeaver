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
        {
            return stringResult;
        }
        throw new InvalidOperationException("What type is that?");
    }

    private record Segment(string content, bool closed);

    private object? GetValue(string key)
    {
        // Check if the input is null, empty, or contains only whitespace
        if (string.IsNullOrWhiteSpace(key))
            return null;

        // List to store the progressively nested representations of the string
        var nestedStrings = new List<string>();
        var nestingLevel = 0; // Track the current nesting level
        var i = 0; // Position in the string for the while loop

        // Build a list of strings, with each entry containing progressively deeper levels of nesting
        while (i < key.Length)
        {
            // Detect the start of a variable {{ by checking two consecutive curly braces
            if (i < key.Length - 1 && key[i] == '{' && key[i + 1] == '{')
                nestingLevel++;  // Increment nesting level when we encounter an opening brace pair

            // Loop over all current levels of nesting and append the current character
            for (var x = 0; x <= nestingLevel; x++)
            {
                // Ensure that the list has enough entries for the current nesting level
                while (nestedStrings.Count <= nestingLevel)
                    nestedStrings.Add(string.Empty);

                // Append the current character to all strings up to the current nesting level
                nestedStrings[x] += key[i];
            }

            // Detect the end of a variable }} and decrease the nesting level
            if (i > 0 && key[i] == '}' && key[i - 1] == '}')
                nestingLevel--;  // Decrement nesting level when we encounter a closing brace pair

            i++; // Move to the next character
        }

        // Now, resolve variables starting from the innermost and move outward
        var level = nestedStrings.Count - 1; // Start from the deepest level of nesting
        while (nestedStrings.Count > 1 && level > 0)
        {
            // Get the current nested string and trim whitespace
            var currentNestedString = nestedStrings[level].Trim();

            // Check if the current string is a valid variable with no further nested variables
            if (currentNestedString.StartsWith("{{") && currentNestedString.IndexOf("{{", 2) == -1)
            {
                // Extract the inner content (removing the {{ and }}), and trim excess spaces
                var variableContent = currentNestedString.TrimStart('{').TrimEnd('}').Trim();

                // Evaluate the content of the variable
                var evaluatedValue = EvaluateValue(variableContent) as string ?? string.Empty;

                // Replace the evaluated value in all higher nesting levels
                for (var y = 0; y < nestedStrings.Count; y++)
                    nestedStrings[y] = nestedStrings[y].Replace(nestedStrings[level], evaluatedValue);

                // Remove the fully resolved nested string, as it's no longer needed
                nestedStrings.RemoveAt(level);
                level = nestedStrings.Count - 1; // Adjust the index for the next iteration
            }
            else
                level--; // Move to the next level up
            
        }

        // After processing, there should be exactly one string left in the list (the fully resolved result)
        if (nestedStrings.Count != 1)
        {
            // If there's more than one unresolved string left, something went wrong
            return null;
        }

        // Return the final fully resolved string
        return nestedStrings.FirstOrDefault();
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