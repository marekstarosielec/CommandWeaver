public interface IInputReader
{
    string? PromptText(string message, bool required, string? promptStyle, char? secretChar = null);
}