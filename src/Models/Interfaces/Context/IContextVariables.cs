namespace Models.Interfaces.Context;

public interface IContextVariables
{
    string CurrentSessionName { get; set; }
    string? CurrentlyProcessedElement { get; set; }
    void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent);
    void SetVariableValue(VariableScope scope, string key, object? value, string? description = null);
    string? GetValueAsString(object? key);
}