public class Output(IOutputWriter outputWriter) : IOutput
{
    public void Debug(string message)
    {
        outputWriter.Write($"[[#FF0000]]{message}[[/]]");
    }

    public void Error(string message)
    {
        outputWriter.Write(message);
    }

    public void Result(string message)
    {
        outputWriter.Write(message);
    }

    public void Trace(string message)
    {
        outputWriter.Write(message);
    }

    public void Warning(string message)
    {
        outputWriter.Write(message);
    }
}

