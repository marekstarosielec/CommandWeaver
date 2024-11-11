/// <summary>
/// Defines a service for handling command definitions.
/// </summary>
public interface ICommands
{
    /// <summary>
    /// Adds a set of commands that can be executed.
    /// </summary>
    /// <param name="commands"></param>
    void Add(IEnumerable<Command> commands);
}
