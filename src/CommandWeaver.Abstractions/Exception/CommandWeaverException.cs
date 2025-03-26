public class CommandWeaverException(string message, int exitCode = 1, Exception? innerException = null) : Exception(message, innerException)
{
    public readonly int ExitCode = exitCode;
}