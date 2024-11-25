/// <summary>
/// Defines a service for managing context-specific variables, including setting, retrieving, and resolving variable values.
/// </summary>
public interface IVariableService
{
    /// <summary>
    /// Gets or sets the name of the current session.
    /// </summary>
    /// <remarks>
    /// This identifies the current session in which variables are being managed, providing context for variable operations.
    /// </remarks>
    string CurrentSessionName { get; set; }

    /// <summary>
    /// Gets or sets the name of the repository currently being loaded.
    /// </summary>
    /// <remarks>
    /// Tracks the active repository to enable contextual variable management and operations specific to the repository.
    /// </remarks>
    string? CurrentlyLoadRepository { get; set; }

    /// <summary>
    /// Gets or sets the name of the element currently being loaded within the repository.
    /// </summary>
    /// <remarks>
    /// Tracks the active repository element for contextual variable management, typically during load or initialization.
    /// </remarks>
    string? CurrentlyLoadRepositoryElement { get; set; }

    /// <summary>
    /// Gets or sets the current logging level.
    /// </summary>
    /// <remarks>
    /// Controls the verbosity of logs related to variable operations and context.
    /// </remarks>
    LogLevel LogLevel { get; set; }

    /// <summary>
    /// Adds a set of variables to a specific repository location.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository where variables will be stored.</param>
    /// <param name="repositoryElementId">The identifier for the specific element within the repository where variables will be stored.</param>
    /// <param name="variables">The list of variables to add to the specified repository location.</param>
    void Add(RepositoryLocation repositoryLocation, string repositoryElementId, IEnumerable<Variable> variables);

    /// <summary>
    /// Retrieves the current repository element storage containing the variable values.
    /// </summary>
    /// <returns>A <see cref="RepositoryElementStorage"/> instance with the current variable values.</returns>
    RepositoryElementStorage GetRepositoryElementStorage();

    /// <summary>
    /// Resolves all variable tags within the specified value.
    /// </summary>
    /// <param name="variableValue">The value to resolve, which may contain variable tags or placeholders.</param>
    /// <param name="treatTextValueAsVariable">
    /// If <c>true</c>, the entire <c>TextValue</c> is treated as a variable name to resolve, rather than resolving individual tags within the value.
    /// </param>
    /// <returns>A <see cref="DynamicValue"/> with all variable references resolved to their corresponding values.</returns>
    DynamicValue ReadVariableValue(DynamicValue variableValue, bool treatTextValueAsVariable = false);

    /// <summary>
    /// Finds a variable by its name within the current context.
    /// </summary>
    /// <param name="variableName">The name of the variable to locate.</param>
    /// <returns>The <see cref="Variable"/> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// This method searches for a variable by its name in the current context, which may include session, command, or repository scopes.
    /// </remarks>
    Variable? FindVariable(string variableName);

    /// <summary>
    /// Writes a specified variable value to a path within a given scope.
    /// </summary>
    /// <param name="scope">The scope within which the variable is stored (e.g., command, session, application).</param>
    /// <param name="path">The path where the variable value will be written.</param>
    /// <param name="value">The value to assign to the variable at the specified path.</param>
    /// <param name="respositoryElementId">Optional identifier of the repository element where the variable will be stored.</param>
    /// <remarks>
    /// This method allows writing or updating a variable value at a specific path and scope, with optional repository-specific storage.
    /// </remarks>
    void WriteVariableValue(VariableScope scope, string path, DynamicValue value, string? respositoryElementId = null); //TODO: Rename to Set and reorder arguments
}
