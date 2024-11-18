using CommandWeaver.Abstractions;

public class Output(IOutputWriter outputWriter) : IOutput
{
    public string? TraceStyle { get; set; }
    public string? DebugStyle { get; set; }
    public string? InformationStyle { get; set; }
    public string? WarningStyle { get; set; }
    public string? ErrorStyle { get; set; }
    public LogLevel CurrentLogLevel { get; set; }
    public ISerializer? Serializer { get; set; }

    public void Trace(string message)=> Write(new DynamicValue($"[[{TraceStyle}]]{message}[[/]]"), LogLevel.Trace, Styling.MarkupLine);
    public void Debug(string message) => Write(new DynamicValue($"[[{DebugStyle}]]{message}[[/]]"), LogLevel.Debug, Styling.MarkupLine);
    public void Information(string message) => Write(new DynamicValue($"[[{InformationStyle}]]{message}[[/]]"), LogLevel.Information, Styling.MarkupLine);
    public void Warning(string message) => Write(new DynamicValue($"[[{WarningStyle}]]{message}[[/]]"), LogLevel.Warning, Styling.MarkupLine);
    public void Error(string message)=> Write(new DynamicValue($"[[{ErrorStyle}]]{message}[[/]]"), LogLevel.Error, Styling.MarkupLine);

    public void Write(DynamicValue value, LogLevel? logLevel, Styling styling)
    {
        if (logLevel is not null && logLevel < CurrentLogLevel)
            return;
        
        if (value.ObjectValue != null || value.ListValue != null)
        {
            if (Serializer == null)
                return;
            if (Serializer.TrySerialize(value, out var result, out _) && !string.IsNullOrEmpty(result))
                outputWriter.WriteJson(result);
        }

        if (value.TextValue != null)
        {
            if (styling == Styling.Raw)
            {
                outputWriter.WriteRaw(value.TextValue);
                return;
            }
            if (styling == Styling.Markup)
            {
                outputWriter.WriteMarkup(value.TextValue);
                return;
            }
            if (styling == Styling.MarkupLine)
            {
                outputWriter.WriteMarkup($"{value.TextValue}{Environment.NewLine}");
                return;
            }
            if (styling == Styling.Json)
            {
                outputWriter.WriteJson(value.TextValue);
                return;
            }

            //Styling == Default
            var text = logLevel switch
            {
                LogLevel.Trace => $"[[{TraceStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Debug => $"[[{DebugStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Information => $"[[{InformationStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Warning => $"[[{WarningStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Error => $"[[{ErrorStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                _ => $"{value.TextValue}{Environment.NewLine}"
            };
            
            outputWriter.WriteMarkup(text);
        }

        if (value.BoolValue.HasValue && value.BoolValue.Value)
            outputWriter.WriteRaw("true");
        if (value.BoolValue.HasValue && !value.BoolValue.Value)
            outputWriter.WriteRaw("false");
    }

    
}

