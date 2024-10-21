using Models.Interfaces.Context;
using Spectre.Console;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutput
{
    public void Trace(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[grey]{message}[/]");
    }

    public void Debug(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[grey]{message}[/]");
    }

    public void Error(string message, int exitCode = 1)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]{message}[/]");
        Environment.Exit(exitCode);
    }

    public void Warning(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[darkgoldenrod]{message}[/]");
    }
}