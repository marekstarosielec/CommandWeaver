using Spectre.Console;
using Spectre.Console.Json;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutputWriter
{
    public void WriteText(string textValue)
    {       
        AnsiConsole.MarkupLineInterpolated(MarkupConverter.Convert(textValue));  
    }

    public void WriteObject(string json)
    {
        AnsiConsole.Write(new JsonText(json));
    }

    public void WriteRaw(string text)
    {
        AnsiConsole.WriteLine(text);
    }
}