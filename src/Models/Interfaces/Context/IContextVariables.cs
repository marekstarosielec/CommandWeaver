namespace Models.Interfaces.Context;

public interface IContextVariables
{
    string CurrentSessionName { get; set; }
    string? CurrentlyProcessedElement { get; set; }
    void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent);

    /// <summary>
    /// Resolves all the variable tags inside given value.
    /// </summary>
    /// <param name="variableValue">Value to resolve. It can be string, object or list.</param>
    /// <param name="treatTextValueAsVariable">Whole TextValue is treated as variable.</param>
    /// <returns></returns>
    VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false);


   

    void SetVariableValue(VariableScope scope, string variableName, object? value, string? description = null);
}