using System.Text;
using System.Text.RegularExpressions;

namespace CommandLine;

public interface IParser
{
    ParserResult ParseFullCommandLine(string commandLine);
}

public readonly struct ParsedElement(string name, string? value, string type)
{
    public string Name { get; } = name;   // Name of the parsed element
    public string? Value { get; } = value; // Nullable value (only for arguments)
    public string Type { get; } = type;   // Type of the element (flag, command, or argument)
}

public struct ParserResult
{
    public required string CommandLine { get; set; }
    public required string Executable { get; set; }
    public required string Arguments { get; set; }
    public required IEnumerable<ParsedElement> ParsedArguments { get; set; }
}

public class Parser : IParser
{
    public ParserResult ParseFullCommandLine(string commandLine)
    {
        var executable = ExtractExecutable(commandLine);
        var arguments = ExtractArguments(commandLine, executable);
        var parsedArguments = ParseArguments(arguments);
        return new ParserResult
        {
            CommandLine = commandLine,
            Executable = executable,
            Arguments = arguments,
            ParsedArguments = parsedArguments
        };
    }

    private string ExtractExecutable(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return string.Empty;
        
        var endingPosition = 0;
        if (commandLine.StartsWith("\""))
            endingPosition = commandLine.IndexOf("\"", 1, StringComparison.Ordinal);
        else if (commandLine.StartsWith("'"))
            endingPosition = commandLine.IndexOf("'", 1, StringComparison.Ordinal);
        else
            endingPosition = commandLine.IndexOf(" ", 1, StringComparison.Ordinal);
        if (endingPosition == -1)
            return commandLine;
        return commandLine.Substring(0, endingPosition);
    }

    private string ExtractArguments(string commandLine, string executable) =>
        commandLine.Substring(executable.Length).Trim();

    internal IEnumerable<ParsedElement> ParseArguments(string input)
    {
        var current = new StringBuilder(); // Holds the current word being processed
        string? lastFlagOrArgument = null; // Tracks the last flag or argument seen
        var inQuotes = false; // Tracks whether we're inside quotes
        var quoteChar = '\0'; // The type of quote being used
        var escapeNext = false; // Tracks if the next character is escaped

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (escapeNext)
            {
                current.Append(c); // Append the escaped character
                escapeNext = false;
                continue;
            }

            if (c == '\\')
            {
                escapeNext = true; // Start escaping the next character
                continue;
            }

            if (inQuotes)
            {
                if (c == quoteChar) // End of quoted section
                {
                    inQuotes = false;
                    quoteChar = '\0';
                }
                else
                    current.Append(c); // Append character inside quotes

                continue;
            }

            if (c == '"' || c == '\'') // Start of a quoted section
            {
                inQuotes = true;
                quoteChar = c;
                continue;
            }

            if (char.IsWhiteSpace(c)) // Handle spaces (end of a word)
            {
                if (current.Length > 0)
                {
                    var word = current.ToString();
                    current.Clear();

                    if (word.StartsWith("--") || word.StartsWith("-"))
                    {
                        // If there's a previously stored flag or argument, yield it as a flag (no value follows)
                        if (lastFlagOrArgument != null)
                            yield return new ParsedElement(lastFlagOrArgument.TrimStart('-'), null, "flag");

                        lastFlagOrArgument = word; // Store this flag or argument
                    }
                    else
                    {
                        // If the last word was a flag/argument, this word is the value
                        if (lastFlagOrArgument != null)
                        {
                            yield return new ParsedElement(lastFlagOrArgument.TrimStart('-'), word, "argument");
                            lastFlagOrArgument = null;
                        }
                        else
                            yield return new ParsedElement(word.TrimStart('-'), null, "command"); // It's a command
                    }
                }

                continue;
            }

            current.Append(c); // Continue building the current word
        }

        // Handle the last word after the loop finishes
        if (current.Length > 0)
        {
            var word = current.ToString();

            if (lastFlagOrArgument != null)
            {
                if (word.StartsWith("-"))
                {
                    // If the last word was a flag/argument and we reached the end, yield it as a flag
                    yield return new ParsedElement(lastFlagOrArgument.TrimStart('-'), word.StartsWith("-") ? null : word, "flag");
                    yield return new ParsedElement(word.TrimStart('-'), null, "flag");
                }
                else
                    yield return new ParsedElement(lastFlagOrArgument.TrimStart('-'), word,"argument");
            }
            else
                // This could be a standalone command or flag
                yield return new ParsedElement(word.TrimStart('-'), null, word.StartsWith("-") ? "flag" : "command");
            
        }
        else if (lastFlagOrArgument != null)
            yield return new ParsedElement(lastFlagOrArgument.TrimStart('-'), null, "flag");
    }
}