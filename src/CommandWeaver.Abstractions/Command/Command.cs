/// <summary>
/// Represents a command consisting of a name, a list of operations to be executed, and a set of parameters.
/// </summary>
public class Command
{
    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    /// <remarks>This is a required field and uniquely identifies the command.</remarks>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the list of operations associated with the command.
    /// </summary>
    /// <remarks>
    /// Each operation in this list defines a specific action or series of actions to be executed as part of the command.
    /// </remarks>
    public List<Operation> Operations { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of parameters associated with the command.
    /// </summary>
    /// <remarks>
    /// These parameters provide additional context or configuration for the command and its operations.
    /// </remarks>
    public List<CommandParameter> Parameters { get; set; } = [];
}
