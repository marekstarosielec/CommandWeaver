public class Output(IOutputWriter outputWriter) : IOutput
{
    public string? DebugStyle { get; set; }
    public string? TraceStyle { get; set; }
    public string? WarningStyle { get; set; }
    public string? ErrorStyle { get; set; }
    public string? ResultStyle { get; set; }

    public void Debug(string message)
    {
        outputWriter.Write($"[[{DebugStyle ?? "#808080"}]]{message}[[/]]");
    }

    public void Error(string message)
    {
        outputWriter.Write($"[[{ErrorStyle ?? "#af0000"}]]{message}[[/]]");
    }

    public void Result(string message, string? format)
    {
        if (format == "raw") 
            outputWriter.WriteRaw(message);
        else
            outputWriter.Write($"[[{ResultStyle ?? ""}]]{message}[[/]]");
    }

    public void Trace(string message)
    {
        outputWriter.Write($"[[{TraceStyle ?? "#c0c0c0"}]]{message}[[/]]");
    }

    public void Warning(string message)
    {
        outputWriter.Write($"[[{WarningStyle ?? "#af8700" }]]{message}[[/]]");
    }
}

