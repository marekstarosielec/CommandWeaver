using Spectre.Console;
using Spectre.Console.Json;

public class SpectreConsoleOutput : IOutputWriter
{
    public void WriteRaw(string textValue)
    {
        AnsiConsole.Console.Write(textValue);
    }

    public void WriteMarkup(string textValue)
    {
        AnsiConsole.Write(new CustomRenderable(MarkupConverter.ConvertToSegments(textValue)));
    }

    public void WriteJson(string json)
    {
        try
        {
            AnsiConsole.Write(new JsonText(json));
            AnsiConsole.WriteLine();
        }
        catch
        {
            AnsiConsole.Console.Write(json);
        }
    }

    public void WriteException(Exception exception)
    {
        AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
    }
}