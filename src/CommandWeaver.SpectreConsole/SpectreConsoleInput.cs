using Spectre.Console;

public class SpectreConsoleInput : IInputReader
{
    public string? PromptText(string message, bool required, string? promptStyle, char? secretChar = null)
    {
        var markup = MarkupConverter.ConvertToSpectreMarkup(message);
        var prompt = new TextPrompt<string>(markup);
        if (!required)
            prompt = prompt.AllowEmpty();
        if (promptStyle != null)
            prompt = prompt.PromptStyle(MarkupConverter.ConvertToSpectreStyle(promptStyle));
        if (secretChar != null)
            prompt = prompt.Secret(secretChar);
        // prompt = prompt.DefaultValue("erter").HideDefaultValue();
        return AnsiConsole.Prompt(prompt);
    }
}