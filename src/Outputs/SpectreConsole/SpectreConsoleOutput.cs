using Spectre.Console;
using Spectre.Console.Json;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutputWriter
{
    public void WriteText(string textValue)
    {       
        AnsiConsole.MarkupLineInterpolated(MarkupConverter.Convert(textValue));  
    }

    public void WriteObject(DynamicValueObject objectValue)
    {
        AnsiConsole.Write(new JsonText("{ \"property\":\"value\"}"));
    }

    public void WriteRaw(string text)
    {
        AnsiConsole.WriteLine(text);
    }
}