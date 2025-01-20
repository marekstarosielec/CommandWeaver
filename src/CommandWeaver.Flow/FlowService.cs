/// <inheritdoc />
public class FlowService(IOutputService outputService) : IFlowService
{
    /// <inheritdoc />
    public void Terminate(string? message = null, int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(message))
            outputService.Error(message);
        Environment.Exit(exitCode);
    }

    /// <inheritdoc />
    public void NonFatalException(Exception? exception)
    {
        if (exception == null)
            return;
        outputService.WriteException(exception);
    }

    /// <inheritdoc />
    public void FatalException(Exception? exception, string? message = null, int exitCode = 1)
    {
        if (exception == null)
            return;
        outputService.WriteException(exception);
        Terminate(message, exitCode);
    }
}
