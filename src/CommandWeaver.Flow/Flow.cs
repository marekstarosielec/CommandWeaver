/// <inheritdoc />
public class Flow(IOutputService output) : IFlowService
{
    /// <inheritdoc />
    public void Terminate(string? message = null, int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(message))
            output.Error(message);
        Environment.Exit(exitCode);
    }
}
