using Spectre.Console;

public class SpectreConsoleInput : IInputReader
{
    public string? Prompt()
    {
        return AnsiConsole.Prompt(new TextPrompt<string>("What's your name?"));
    }
}