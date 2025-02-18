public class OutputService(IOutputWriter outputWriter, IOutputSettings outputSettings) : IOutputService
{
    public void Trace(string message)=> Write(new DynamicValue(message), LogLevel.Trace, Styling.Default);
    public void Debug(string message) => Write(new DynamicValue(message), LogLevel.Debug, Styling.Default);
    public void Information(string message) => Write(new DynamicValue(message), LogLevel.Information, Styling.Default);
    public void Warning(string message) => Write(new DynamicValue(message), LogLevel.Warning, Styling.Default);
    public void Error(string message)=> Write(new DynamicValue(message), LogLevel.Error, Styling.Default);

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
        
        if (value.NumericValue != null)
            outputWriter.WriteMarkup(value.NumericValue.Value.ToString());
    }

    public void WriteException(Exception exception) => outputWriter.WriteException(exception);

    // public async Task WriteRequest(HttpRequestMessage request)
    // {
    //     outputWriter.WriteMarkup($"[[{outputSettings.Styles?["trace"]}]]----------------------------------------------------------------------------------[[/]]\r\n");
    //     outputWriter.WriteMarkup($"[[#005f87]]{request.Method.Method}[[/]]");
    //     outputWriter.WriteMarkup($" {request.RequestUri?.AbsoluteUri}");
    //     outputWriter.WriteMarkup($" [[{outputSettings.Styles?["trace"]}]]({DateTime.UtcNow:o})[[/]]\r\n\r\n");
    //     var headers = request.Headers.ToList().Concat(request.Content?.Headers.ToList() ?? []);
    //     foreach (var header in headers)
    //     {
    //         outputWriter.WriteMarkup($"[[{outputSettings.Styles?["trace"]}]]{header.Key}: [[/]]");
    //         outputWriter.WriteMarkup($"[[{outputSettings.Styles?["debug"]}]]{string.Join(',', header.Value)}[[/]]\r\n");
    //     }
    //     outputWriter.WriteMarkup("\r\n");
    //     if (request.Content == null)
    //         return;
    //     
    //     var body = await request.Content.ReadAsStringAsync();
    //     if (!JsonHelper.IsJson(body))
    //         outputWriter.WriteRaw(body);
    //     else
    //         outputWriter.WriteJson(body);
    //     
    //     outputWriter.WriteMarkup("\r\n");
    // }
    //
    // public async Task WriteResponse(HttpResponseMessage response)
    // {
    //     outputWriter.WriteMarkup($"[[{outputSettings.Styles?["trace"]}]]----------------------------------------------------------------------------------[[/]]\r\n");
    //     outputWriter.WriteMarkup($"[[#005f87]]{(int) response.StatusCode}[[/]]");
    //     outputWriter.WriteMarkup($" {response.RequestMessage?.RequestUri?.AbsoluteUri}");
    //     outputWriter.WriteMarkup($" [[{outputSettings.Styles?["trace"]}]]({DateTime.UtcNow:o})[[/]]\r\n\r\n");
    //     var headers = response.Headers.ToList().Concat(response.Content?.Headers.ToList() ?? []);
    //     foreach (var header in headers)
    //     {
    //         outputWriter.WriteMarkup($"[[{outputSettings.Styles?["trace"]}]]{header.Key}: [[/]]");
    //         outputWriter.WriteMarkup($"[[{outputSettings.Styles?["debug"]}]]{string.Join(',', header.Value)}[[/]]\r\n");
    //     }
    //     outputWriter.WriteMarkup("\r\n");
    //     
    //     if (response.Content == null)
    //         return;
    //     
    //     var body = await response.Content.ReadAsStringAsync();
    //     if (!JsonHelper.IsJson(body))
    //         outputWriter.WriteRaw(body);
    //     else
    //         outputWriter.WriteJson(body);
    //     
    //     outputWriter.WriteMarkup("\r\n");
    // }

    private string TraceStyle => outputSettings.Styles?["trace"] ?? "#c0c0c0";
    private string DebugStyle => outputSettings.Styles?["debug"] ?? "#808080";
    private string InformationStyle => outputSettings.Styles?["information"] ?? "#ffffff";
    private string WarningStyle => outputSettings.Styles?["warning"] ?? "#af8700";
    private string ErrorStyle => outputSettings.Styles?["error"] ?? "#af0000";
}

