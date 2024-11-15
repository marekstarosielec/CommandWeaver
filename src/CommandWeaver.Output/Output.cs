public class Output(IOutputWriter outputWriter) : IOutput
{
    public string? DebugStyle { get; set; }
    public string? TraceStyle { get; set; }
    public string? WarningStyle { get; set; }
    public string? ErrorStyle { get; set; }
    public string? ResultStyle { get; set; }
    public string? LogLevel { get; set; }

    private string GetLogLevel() => LogLevel ?? "information";

    public void Debug(string message)
    {
        if (GetLogLevel() != "debug")
            return;
        outputWriter.WriteText($"[[{DebugStyle ?? "#808080"}]]{message}[[/]]");   
    }

    public void Error(string message)
    {
        outputWriter.WriteText($"[[{ErrorStyle ?? "#af0000"}]]{message}[[/]]");
    }

    public void Result(string message, string? format)
    {
        if (format == "raw") 
            outputWriter.WriteRaw(message);
        else
            outputWriter.WriteText($"[[{ResultStyle ?? ""}]]{message}[[/]]");
    }

    public void Test(DynamicValue value)
    {
        if (value.TextValue != null)
            outputWriter.WriteText(value.TextValue);
        if (value.BoolValue != null)
            outputWriter.WriteText(value.BoolValue.ToString());
        
        if (value.ObjectValue != null)
            outputWriter.WriteObject(value.ObjectValue);
    }

    public void Trace(string message)
    {
        if (GetLogLevel() == "information" || GetLogLevel() == "warning" || GetLogLevel() == "error")
            return;

        outputWriter.WriteText($"[[{TraceStyle ?? "#c0c0c0"}]]{message}[[/]]");
    }

    public void Warning(string message)
    {
        if (GetLogLevel() == "error")
            return;

        outputWriter.WriteText($"[[{WarningStyle ?? "#af8700" }]]{message}[[/]]");
    }
}

