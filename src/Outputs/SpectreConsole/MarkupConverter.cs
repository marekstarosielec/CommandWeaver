using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

internal static class MarkupConverter
{
    public static FormattableString Convert(string input)
    {
        // Initialize the format string and arguments list
        var formatSegments = new List<string>();
        var arguments = new List<object>();
        int lastIndex = 0;

        // Use Regex to find all instances of [[...]]
        var matches = Regex.Matches(input, @"\[\[(.*?)\]\]");
        foreach (Match match in matches)
        {
            // Add preceding static text as a segment
            if (match.Index > lastIndex)
            {
                formatSegments.Add("{" + arguments.Count + "}");  // Use a numbered placeholder
                arguments.Add(input.Substring(lastIndex, match.Index - lastIndex));
            }

            // Process and add the matched tag content as a dynamic argument
            object processedTag = ProcessTag(match.Groups[1].Value);
            //formatSegments.Add("{" + arguments.Count + "}");  // Use a numbered placeholder
            //arguments.Add(processedTag);
            formatSegments.Add(processedTag.ToString());
            lastIndex = match.Index + match.Length;
        }

        // Add the remaining static text after the last match
        if (lastIndex < input.Length)
        {
            formatSegments.Add("{" + arguments.Count + "}");
            arguments.Add(input.Substring(lastIndex));
        }

        // Create the final FormattableString
        var result = FormattableStringFactory.Create(string.Join("", formatSegments), arguments.ToArray());
        return result;
    }

    // Define a function to process each matched element
    static object ProcessTag(string tagContent)
    {
        var parts = tagContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();

            // Custom rules for translating elements
            if (parts[i].StartsWith("#"))
            {
                //parts[i] = parts[i].ToUpper(); // Example transformation
            }
            else if (parts[i].Equals("underline", StringComparison.OrdinalIgnoreCase))
            {
              //  parts[i] = "UL"; // Example transformation
            }
        }

        return $"[{string.Join(" ", parts)}]";
    }
}
