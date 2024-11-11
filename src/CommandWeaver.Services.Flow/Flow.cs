/// <inheritdoc />
public class Flow(IOutput output) : IFlow
{
    /// <inheritdoc />
    public void Terminate(string? message = null, int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(message))
            output.Error(message);
        Environment.Exit(exitCode);
    }
}
