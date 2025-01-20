using System.Text;

public class Parser
{
    public void ParseFullCommandLine(string commandLine, out string command, out Dictionary<string, string> arguments)
    {
        var commandLineExecutable = ExtractExecutable(commandLine);
        var commandLineArguments = ExtractArguments(commandLine, commandLineExecutable);
        var parsedArguments = ParseArguments(commandLineArguments);
        command = string.Join(' ', parsedArguments.Where(p => p.Type == "command").Select(p => p.Name));
        arguments = parsedArguments.Where(p => p.Type == "argument").ToDictionary(a => a.Name, a => a.Value ?? string.Empty);
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
        var wordBuilder = new StringBuilder(); // Holds the current word being processed
        string? previousWord = null; // Tracks the last flag or argument seen
        var inQuotes = false; // Tracks whether we're inside quotes
        var quoteChar = '\0'; // The type of quote being used
        var escapeNext = false; // Tracks if the next character is escaped
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (escapeNext)
            {
                wordBuilder.Append(c); // Append the escaped character
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
                    wordBuilder.Append(c); // Append character inside quotes

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
                if (wordBuilder.Length > 0)
                {
                    var word = wordBuilder.ToString();
                    wordBuilder.Clear();

                    if (word.StartsWith("--") || word.StartsWith("-"))
                    {
                        // If there's a previously stored flag or argument, yield it as a argument with default true
                        if (previousWord != null)
                            yield return new ParsedElement(previousWord.TrimStart('-'), "true", "argument");

                        previousWord = word; // Store this flag or argument
                    }
                    else
                    {
                        // If the last word was a flag/argument, this word is the value
                        if (previousWord != null)
                        {
                            yield return new ParsedElement(previousWord.TrimStart('-'), word, "argument");
                            previousWord = null;
                        }
                        else
                            yield return new ParsedElement(word.TrimStart('-'), null, "command"); // It's a command
                    }
                }

                continue;
            }

            wordBuilder.Append(c); // Continue building the current word
        }

        // Handle the last word after the loop finishes
        if (wordBuilder.Length > 0)
        {
            var word = wordBuilder.ToString();

            if (previousWord == null)
            { 
                // This could be a standalone command or flag
                yield return new ParsedElement(word.TrimStart('-'), word.StartsWith("-") ? "true" : null, word.StartsWith("-") ? "argument" : "command");
                yield break;
            }
            if (word.StartsWith("-"))
            {
                // If the last word was a flag/argument and we reached the end, yield it as a flag
                yield return new ParsedElement(previousWord.TrimStart('-'), word.StartsWith("-") ? "true" : word, "argument");
                yield return new ParsedElement(word.TrimStart('-'), "true", "argument");
            }
            else
                yield return new ParsedElement(previousWord.TrimStart('-'), word, "argument");
               
        }
        else if (previousWord != null)
            yield return new ParsedElement(previousWord.TrimStart('-'), "true", "argument");
    }
}