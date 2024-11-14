using Spectre.Console;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutputWriter
{
    //public void Trace(string message)
    //{
    //    AnsiConsole.MarkupLineInterpolated($"[grey]{message}[/]");
    //}

    //public void Debug(string message)
    //{
    //    AnsiConsole.MarkupLineInterpolated($"[grey]{message}[/]");
    //}

    //public void Result(string message)
    //{
    //    AnsiConsole.MarkupLineInterpolated($"[blue]{message}[/]");
    //}

    //public void Error(string message)
    //{
    //    AnsiConsole.MarkupLineInterpolated($"[red]{message}[/]");
    //}

    //public void Warning(string message)
    //{
    //    AnsiConsole.MarkupLineInterpolated($"[darkgoldenrod]{message}[/]");
    //}

    public void Write(string text)
    {
        AnsiConsole.MarkupLineInterpolated(MarkupConverter.Convert(text));  
    }
}