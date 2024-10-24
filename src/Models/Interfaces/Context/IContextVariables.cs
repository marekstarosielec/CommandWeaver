namespace Models.Interfaces.Context;

public interface IContextVariables
{
    string CurrentSessionName { get; set; }
    string? CurrentlyProcessedElement { get; set; }
    void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent);
    
    string? GetValueAsString(object? key, bool asVariable = false);
    int? GetValueAsInt(object? key, bool asVariable = false);
    VariableValueObject? GetValueAsObject(object? key, bool asVariable = false);
    VariableValueList? GetValueAsList(object? key, bool asVariable = false);
    VariableValue GetValue(VariableValue? variableValue, bool asVariable = false);

    void SetVariableValue(VariableScope scope, string variableName, object? value, string? description = null);
}