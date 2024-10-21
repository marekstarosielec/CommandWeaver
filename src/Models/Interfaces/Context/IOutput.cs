namespace Models.Interfaces.Context;

public interface IOutput
{
    void Debug(string message);
    
    void Warning(string message);

    void Error(string message, int exitCode = 1);
}