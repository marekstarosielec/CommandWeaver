public interface IOutput
{
    void Trace(string message);

    void Debug(string message);
    
    void Warning(string message);

    void Error(string message, int exitCode = 1);
}