
namespace Models.Interfaces;

/// <summary>
/// Allows to crawl in nested values in variables.
/// </summary>
public interface IVariableValueCrawler
{
    /// <summary>
    /// Find value of variable by key. It can be nested inside.
    /// </summary>
    /// <param name="variables">List of variables. First element from key</param>
    /// <param name="key">Full key in form variable.property[listIndex].subProperty</param>
    /// <returns></returns>
    object? GetSubValue(List<Variable> variables, string key);
}
