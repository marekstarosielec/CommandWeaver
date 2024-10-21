using Models.Interfaces.Context;
using Spectre.Console;

namespace SpectreConsole;

public class SpectreConsoleOutput : IOutput
{
    public void Debug(string message)
    {
        AnsiConsole.MarkupLine($"[grey]{message}[/]");
    }

    public void Error(string message, int exitCode = 1)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
        Environment.Exit(exitCode);
    }

    public void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[darkgoldenrod]{message}[/]");
    }
}