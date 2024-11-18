/// <summary>
/// Defines a service for managing context-specific variables, including setting, retrieving, and resolving variable values.
/// </summary>
public interface IVariables
{
    /// <summary>
    /// Gets or sets the name of the current session.
    /// </summary>
    /// <remarks>This identifies the current session in which variables are being managed.</remarks>
    string CurrentSessionName { get; set; }

    /// <summary>
    /// Gets or sets the name of the repository currently being loaded.
    /// </summary>
    /// <remarks>Used to track the active repository, allowing for contextual variable management.</remarks>
    string? CurrentlyLoadRepository { get; set; }

    /// <summary>
    /// Gets or sets the name of the element currently being loaded.
    /// </summary>
    /// <remarks>Used to track the active element, allowing for contextual variable management.</remarks>
    string? CurrentlyLoadRepositoryElement { get; set; }

    LogLevel LogLevel { get; set; }

    /// <summary>
    /// Adds a set of variables in a specified repository location.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository where variables will be stored.</param>
    /// <param name="variables">The list of variables to store in the repository location.</param>
    /// <param name="repositoryElementId">The identifier for the specific location within the repository.</param>
    void Add(RepositoryLocation repositoryLocation, IEnumerable<Variable> variables, string repositoryElementId);

    /// <summary>
    /// Retrieves repository elemnt storage with current variables values.
    /// </summary>
    /// <returns>
    /// </returns>
    RepositoryElementStorage GetRepositoryElementStorage();

    /// <summary>
    /// Resolves all variable tags within the specified value.
    /// </summary>
    /// <param name="variableValue">The value to resolve, which may be a string, object, or list.</param>
    /// <param name="treatTextValueAsVariable">If <c>true</c>, the entire <c>TextValue</c> is treated as a variable to resolve.</param>
    /// <returns>A <see cref="DynamicValue"/> with resolved variable values.</returns>
    DynamicValue ReadVariableValue(DynamicValue variableValue, bool treatTextValueAsVariable = false);

    /// <summary>
    /// Finds a variable by its name within the context.
    /// </summary>
    /// <param name="variableName">The name of the variable to locate.</param>
    /// <returns>The <see cref="Variable"/> if found; otherwise, <c>null</c>.</returns>
    Variable? FindVariable(string variableName);

    /// <summary>
    /// Writes a specified variable value to a path within a given scope.
    /// </summary>
    /// <param name="scope">The scope within which the variable is stored (e.g., command, session, application).</param>
    /// <param name="path">The path where the variable value will be written.</param>
    /// <param name="value">The value to assign to the variable at the specified path.</param>
    /// <param name="respositoryElementId">Optional id of repository where variable will be stored.</param>
    void WriteVariableValue(VariableScope scope, string path, DynamicValue value, string? respositoryElementId = null); //TODO: Rename to Set and reorder arguments
}
