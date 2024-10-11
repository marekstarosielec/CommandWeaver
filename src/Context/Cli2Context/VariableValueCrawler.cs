using Models;
using Models.Interfaces;
using System.Text.RegularExpressions;

namespace Cli2Context;

/// <inheritdoc />
public class VariableValueCrawler() : IVariableValueCrawler
{
    /// <inheritdoc />
    public object? GetSubValue(List<Variable> variables, string key)
    {
        object? result = null;
        var pattern = @"([a-zA-Z0-9_\-\s]+)|\[(.*?)]"; 
        var matches = Regex.Matches(key, pattern);

       
        for (int i = 0; i < matches.Count; i++)
        {
            if (i == 0 && matches[i].Groups[1].Success)
                result = variables.FirstOrDefault(v => v.Key.Equals(matches[i].Groups[1].Value, StringComparison.InvariantCultureIgnoreCase))?.Value;
            else if (i == 0 && matches[i].Groups[2].Success)
                //Key cannot start with index. Abort.
                return null;
            else if (!matches[i].Groups[1].Success && !matches[i].Groups[2].Success)
                //Invalid element in key.
                return null;
            else if (i > 0 && matches[i].Groups[1].Success)
                //Go to subproperty
                result = (result as Dictionary<string, object>)?[matches[i].Groups[1].Value];
            else if (i > 0 && matches[i].Groups[2].Success)
                //Go to element in list
                result = (result as List<Dictionary<string, object?>>)?.FirstOrDefault(v => (v["key"] as string)?.Equals(matches[i].Groups[2].Value, StringComparison.InvariantCultureIgnoreCase) == true);
            if (result == null)
                return result;
        }
        return result;
    }
}
