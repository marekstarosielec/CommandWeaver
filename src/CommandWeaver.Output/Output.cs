public class Output(IOutputWriter outputWriter) : IOutput
{
    public void Debug(string message)
    {
        outputWriter.Write($"[[#808080]]{message}[[/]]");
    }

    public void Error(string message)
    {
        outputWriter.Write($"[[#af0000]]{message}[[/]]");
    }

    public void Result(string message)
    {
        outputWriter.Write(message);
    }

    public void Trace(string message)
    {
        outputWriter.Write($"[[#c0c0c0]]{message}[[/]]");
    }

    public void Warning(string message)
    {
        outputWriter.Write($"[[#af8700]]{message}[[/]]");
    }
}

