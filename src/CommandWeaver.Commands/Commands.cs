/// <inheritdoc />
public class Commands : ICommands
{
    private List<Command> _commands = [];

    /// <inheritdoc />
    public void Add(IEnumerable<Command> commands)
    {
        _commands.AddRange(commands);
    }
}

