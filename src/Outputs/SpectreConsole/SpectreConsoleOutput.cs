using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutputWriter
{
    public void WriteText(string textValue)
    {
        // var segments = new List<Segment>
        // {
        //     new Segment("This is my text", new Style(new Color(255, 105, 180), null, link: "https://google.com")),  // Main text
        //     new Segment("This is my text2", new Style(Color.Blue)),
        //     new Segment("This is my text3")
        // };
        //
        // AnsiConsole.Write(new CustomRenderable(segments));
        // //
        // // var t = MarkupConverter.Convert(textValue);
        // //
        // var t = MarkupConverter.ConvertToSegments(textValue);
        // AnsiConsole.MarkupLineInterpolated(MarkupConverter.Convert(textValue));  
        AnsiConsole.Write(new CustomRenderable(MarkupConverter.ConvertToSegments(textValue)));
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