/// <summary>
/// Specifies the scope of a variable, indicating its lifespan and persistence within the system.
/// </summary>
public enum VariableScope
{
    /// <summary>
    /// The variable is scoped to a single command execution.
    /// </summary>
    /// <remarks>
    /// Variables with this scope are not persisted and are only available for the duration of the command execution.
    /// </remarks>
    Command = 0,

    /// <summary>
    /// The variable is scoped to the session.
    /// </summary>
    /// <remarks>
    /// Variables with this scope persist for the duration of the session and are cleared when the session ends.
    /// </remarks>
    Session = 1,

    /// <summary>
    /// The variable is scoped to the entire application.
    /// </summary>
    /// <remarks>
    /// Variables with this scope are available across sessions and persist for the entire application lifecycle.
    /// </remarks>
    Application = 2
}
