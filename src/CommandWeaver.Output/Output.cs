public class Output(IOutputWriter outputWriter, IOutputSettings outputSettings) : IOutput
{
    public void Trace(string message)=> Write(new DynamicValue($"[[{outputSettings.TraceStyle}]]{message}[[/]]"), LogLevel.Trace, Styling.MarkupLine);
    public void Debug(string message) => Write(new DynamicValue($"[[{outputSettings.DebugStyle}]]{message}[[/]]"), LogLevel.Debug, Styling.MarkupLine);
    public void Information(string message) => Write(new DynamicValue($"[[{outputSettings.InformationStyle}]]{message}[[/]]"), LogLevel.Information, Styling.MarkupLine);
    public void Warning(string message) => Write(new DynamicValue($"[[{outputSettings.WarningStyle}]]{message}[[/]]"), LogLevel.Warning, Styling.MarkupLine);
    public void Error(string message)=> Write(new DynamicValue($"[[{outputSettings.ErrorStyle}]]{message}[[/]]"), LogLevel.Error, Styling.MarkupLine);

    public void Write(DynamicValue value, LogLevel? logLevel, Styling styling)
    {
        if (logLevel is not null && logLevel < outputSettings.CurrentLogLevel)
            return;
        
        if (value.ObjectValue != null || value.ListValue != null)
        {
            if (outputSettings.Serializer == null)
                return;
            if (outputSettings.Serializer.TrySerialize(value, out var result, out _) && !string.IsNullOrEmpty(result))
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
                LogLevel.Trace => $"[[{outputSettings.TraceStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Debug => $"[[{outputSettings.DebugStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Information => $"[[{outputSettings.InformationStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Warning => $"[[{outputSettings.WarningStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
                LogLevel.Error => $"[[{outputSettings.ErrorStyle}]]{value.TextValue}[[/]]{Environment.NewLine}",
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

