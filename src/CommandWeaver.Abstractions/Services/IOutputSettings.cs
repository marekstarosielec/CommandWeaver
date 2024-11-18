public interface IOutputSettings
{
    string? TraceStyle { get; set; }
    string? DebugStyle { get; set; }
    string? InformationStyle { get; set; }
    string? WarningStyle { get; set; }
    string? ErrorStyle { get; set; }
    LogLevel CurrentLogLevel { get; set; }
    ISerializer? Serializer { get; set; }
}