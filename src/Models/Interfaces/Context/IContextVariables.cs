namespace Models.Interfaces.Context;

public interface IContextVariables
{
    string CurrentSessionName { get; set; }
    string? CurrentlyProcessedElement { get; set; }
    void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent, string locationId);

    /// <summary>
    /// Resolves all the variable tags inside given value.
    /// </summary>
    /// <param name="variableValue">Value to resolve. It can be string, object or list.</param>
    /// <param name="treatTextValueAsVariable">Whole TextValue is treated as variable.</param>
    /// <returns></returns>
    DynamicValue? ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable = false);

    Variable? FindVariable(string variableName);
   

    void WriteVariableValue(VariableScope scope, string path, DynamicValue value);
}