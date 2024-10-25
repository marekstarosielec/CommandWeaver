using System.Text.RegularExpressions;
using Models;
using Models.Interfaces.Context;

namespace Cli2Context;

public class ContextVariables(IOutput output) : IContextVariables
{
    internal readonly List<Variable> _builtIn = new();
    internal readonly List<Variable> _local = new();
    internal readonly List<Variable> _session = new();
    internal readonly List<Variable> _changes = new();

    public string CurrentSessionName
    {
        get => ResolveTextKey("currentSessionName", true)?.TextValue ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => ResolveTextKey("currentlyProcessedElement", true)?.TextValue;
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
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    /// <inheritdoc />
    public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
    {
        if (variableValue == null)
            return null;
        var result = variableValue with { };
        if (result.TextValue != null)
            result = ResolveTextKey(variableValue.TextValue, treatTextValueAsVariable) ?? variableValue;
        if (result?.ObjectValue != null)
            result = ResolveObjectKey(result.ObjectValue);
        if (result?.ListValue != null)
            result = ResolveListKey(result.ListValue);

        return result;
    }

   
    /// <summary>
    /// Resolves text key. 
    /// </summary>
    /// <param name="key">Text value that might contain variable tags.</param>
    /// <returns>Resolved text, object or list.</returns>
    private VariableValue? ResolveTextKey(string? key, bool treatTextValueAsVariable)
    {
        // Check if the input is null, empty, or contains only whitespace
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = treatTextValueAsVariable ? $"{{{{ { key } }}}}" : key;
        var variableName = ExtractVariableBetweenDelimiters(resolvedKey);
        if (variableName == null)
            return new VariableValue(key);

        var resolvedVariable = ResolveSingleValue(variableName);
        if (resolvedVariable == null)
            return null;
        
        var wholeKeyIsSingleVariable = Regex.Match(resolvedKey, @$"^\{{{{\s*{Regex.Escape(variableName)}\s*\}}}}$") != Match.Empty;
        if (wholeKeyIsSingleVariable)
            //If whole key contains just single variable, it can be replaced with text, object or list.
            return ResolveVariableValue(resolvedVariable);
        if (resolvedVariable?.TextValue != null)
        {
            // Pattern to match {{ variableName }} with possible whitespaces
            string pattern = $@"\{{\{{\s*{Regex.Escape(variableName)}\s*\}}\}}";

            // Replace all occurrences with the variable value
            resolvedKey = Regex.Replace(resolvedKey, pattern, resolvedVariable.TextValue);
            return ResolveVariableValue(new VariableValue(resolvedKey));
        }
        output.Error($"{{{{ {variableName} }}}} resolved to non text value, it cannot be part of text");
        return null;
    }

    /// <summary>
    /// Resolves object key. 
    /// </summary>
    /// <param name="key">VariableValueObject value that has properties that might contain variable tags.</param>
    /// <returns>Resolved object</returns>
    private VariableValue? ResolveObjectKey(VariableValueObject? key)
    {
        if (key == null) 
            return null;
        Dictionary<string, VariableValue?> result = new ();
        foreach (var keyProperty in key.Keys)
            result[keyProperty] = ResolveVariableValue(key[keyProperty]);

        return new VariableValue(new VariableValueObject(result));
    }

    /// <summary>
    /// Resolves list key. 
    /// </summary>
    /// <param name="key">VariableValueList value that has elements that has properties that might contain variable tags.</param>
    /// <returns>Resolved list</returns>
    private VariableValue? ResolveListKey(VariableValueList? key)
    {
        if (key == null)
            return null;
        List<Dictionary<string, VariableValue?>> result = new ();
        foreach (var listElement in key)
        {
            Dictionary<string, VariableValue?> resultListElement = new ();
            foreach (var keyProperty in listElement.Keys)
                resultListElement[keyProperty] = ResolveVariableValue(listElement[keyProperty]);
            result.Add(resultListElement);
        }
        return new VariableValue(new VariableValueList(result));
    }


    /// <summary>
    /// Gets the value of variable by given key. Key cannot contain any variables inside.
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    private VariableValue? ResolveSingleValue(string variableName)
    { 
        var builtIn = GetSubValue(_builtIn, variableName);
        var local = GetSubValue(_local, variableName);
        var session = GetSubValue(_session, variableName);
        var changes = GetSubValue(_changes, variableName);

        //var isList = false;
        //if (KeyIsTopLevel(key) &&
        //    (builtIn?.ListValue != null 
        //    || local?.ListValue != null
        //    || session?.ListValue != null
        //    || changes?.ListValue != null))
        //    isList = true; //If whole variable value is requested and it is list, values from all scopes will be combined.
       
        //if (!isList)
            return changes ?? session ?? local ?? builtIn; 
        //else
        //{
        //    //Return all rows
        //    var result = new VariableValueList();
        //    if (changes?.ListValue != null)
        //        foreach (var item in changes.ListValue)
        //            result.Add(item);

        //    if (session?.ListValue != null)
        //        foreach (var item in session.ListValue)
        //            if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                result.Add(item);

        //    if (local?.ListValue != null)
        //        foreach (var item in local.ListValue)
        //            if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                result.Add(item);
            
        //    if (builtIn?.ListValue != null)
        //        foreach (var item in builtIn.ListValue)
        //            if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                result.Add(item);

        //    return new VariableValue { ListValue = result };
        //}
    }

    /// <summary>
    /// Find value of variable by key. It can be nested inside.
    /// </summary>
    /// <param name="variables">List of variables. First element from key</param>
    /// <param name="key">Full key in form variable.property[listIndex].subProperty</param>
    /// <returns></returns>
    private VariableValue? GetSubValue(List<Variable> variables, string key)
    {
        VariableValue? result = null;
        var pattern = @"([a-zA-Z0-9_\-\s]+)|\[(.*?)]";
        var matches = Regex.Matches(key, pattern);


        for (int i = 0; i < matches.Count; i++)
        {
            if (i == 0 && matches[i].Groups[1].Success)
                result = variables.FirstOrDefault(v => v.Key.Equals(matches[i].Groups[1].Value, StringComparison.InvariantCultureIgnoreCase))?.Value;
            else if (i == 0 && matches[i].Groups[2].Success)
            {
                //Key cannot start with index. Abort.
                output.Error($"Invalid key {key}");
                return null;
            }
            else if (!matches[i].Groups[1].Success && !matches[i].Groups[2].Success)
            {
                //Invalid element in key.
                output.Error($"Invalid key {key}");
                return null;
            }
            else if (i > 0 && matches[i].Groups[1].Success)
                //Go to subproperty
                result = result?.ObjectValue?[matches[i].Groups[1].Value];
            else if (i > 0 && matches[i].Groups[2].Success)
                //Go to element in list
                result = new VariableValue(result?.ListValue?.FirstOrDefault(v => v["key"].TextValue?.Equals(matches[i].Groups[2].Value, StringComparison.InvariantCultureIgnoreCase) == true));
            if (result == null)
                return result;
        }

        return result;
    }


    /// <summary>
    /// Extracts the string between specified delimiters within a given input string.
    /// </summary>
    /// <param name="input">The input string from which to extract the variable.</param>
    /// <returns>
    /// The extracted string between the delimiters, or null if the delimiters are not found.
    /// </returns>
    /// <remarks>
    /// This method currently looks for the delimiters "{{" and "}}", but the delimiters may be customized in future versions.
    /// </remarks>
    private string? ExtractVariableBetweenDelimiters(string input)
    {
        // Find the position of the closing braces }}
        int closingIndex = input.IndexOf("}}");
        if (closingIndex == -1) return null;

        // Find the position of the opening braces {{ before the closing braces
        int openingIndex = input.LastIndexOf("{{", closingIndex);
        if (openingIndex == -1) return null;

        // Extract the string between {{ and }}
        return input.Substring(openingIndex + 2, closingIndex - openingIndex - 2).Trim();
    }

    

    /// <summary>
    /// Indicates if key point to top level of variable value (whole variable value).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private bool KeyIsTopLevel(string key) => !key.Contains(".") && !key.Contains("[");

    /// <summary>
    /// Returns variable name part of key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetTopLevel(string key) => key.Split(['.', '['], StringSplitOptions.RemoveEmptyEntries).First();

    public void SetVariableValue(VariableScope scope, string variableName, object? value, string? description = null)
    {
        //var topLevel = GetTopLevel(variableName);
        //var existingVariable = _changes.FirstOrDefault(v =>
        //    v.Key.Equals(topLevel, StringComparison.InvariantCultureIgnoreCase) && v.Scope == scope);

        //if (variableName != topLevel && existingVariable == null) //new element in list
        //{
        //    if (value is VariableValueObject objectValue)
        //        _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = new VariableValueList { objectValue } }, Scope = scope, Description = description });
        //    else if (value is VariableValueList listValue)
        //        _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = listValue }, Scope = scope, Description = description });
        //    else
        //        output.Error("Tried to insert wrong type into list");
        //}
        //else if (variableName == topLevel && existingVariable == null) //new variable
        //    //Add other types of new value
        //    _changes.Add(new Variable { Key = variableName, Value = new VariableValue { TextValue = (string) value }, Scope = scope, Description = description });
        //else if (existingVariable != null) //existing variable
        //    existingVariable.Value = new VariableValue { TextValue = (string)value };
    }
}