using Spectre.Console;

public class SpectreConsoleInput : IInputReader
{
    public string? Prompt(string message)
    {
        // Merge with Output project
        // Support for markup in questions
        return AnsiConsole.Prompt(new TextPrompt<string>(message));
    }
}