namespace Models.Interfaces.Context;

public interface IContextVariables
{
    string CurrentSessionName { get; set; }
    string? CurrentlyProcessedElement { get; set; }
    void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent);
    Variable? GetVariableValue(string key);
    void SetVariableValue(VariableScope scope, string key, object? value, string? description = null);
    object? GetVariableValue2(string key);
}