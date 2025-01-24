using System.Text.Json;

public static class JsonHelper
{
    /// <summary>
    /// Returns <c>true</c> if provided input is valid json. Otherwise, returns <c>false</c>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsJson(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();
        if ((!input.StartsWith("{") || !input.EndsWith("}")) &&
            (!input.StartsWith("[") || !input.EndsWith("]"))) return false;
        
        try
        {
            using var doc = JsonDocument.Parse(input, new JsonDocumentOptions { AllowTrailingCommas = true });
            return true;
        }
        catch
        {
            // ignored
        }

        return false;
    }

    /// <summary>
    /// Default extension for json files.
    /// </summary>
    public static string Extension = "json";
}