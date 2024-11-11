/// <inheritdoc />
public class CommandWeaver(ILoader loader) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        await loader.Execute(cancellationToken);
        throw new NotImplementedException();
    }

    
}

