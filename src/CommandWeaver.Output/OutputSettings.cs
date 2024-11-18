public class OutputSettings : IOutputSettings
{
    public string? TraceStyle { get; set; }
    public string? DebugStyle { get; set; }
    public string? InformationStyle { get; set; }
    public string? WarningStyle { get; set; }
    public string? ErrorStyle { get; set; }
    public LogLevel CurrentLogLevel { get; set; }
    public ISerializer? Serializer { get; set; }
}