using Spectre.Console;

public class SpectreConsoleInput : IInputReader
{
    public string? PromptText(string message, bool required, string? promptStyle)
    {
        var markup = MarkupConverter.ConvertToSpectreMarkup(message);
        var prompt = new TextPrompt<string>(markup);
        if (!required)
            prompt = prompt.AllowEmpty();
        if (promptStyle != null)
            prompt = prompt.PromptStyle(MarkupConverter.ConvertToSpectreStyle(promptStyle));
        return AnsiConsole.Prompt(prompt);
    }
}