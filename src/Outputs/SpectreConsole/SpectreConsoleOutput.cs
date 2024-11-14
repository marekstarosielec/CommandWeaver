using Spectre.Console;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutputWriter
{
    public void Write(string text)
    {       
        AnsiConsole.MarkupLineInterpolated(MarkupConverter.Convert(text));  
    }

    public void WriteRaw(string text)
    {
        AnsiConsole.WriteLine(text);
    }
}