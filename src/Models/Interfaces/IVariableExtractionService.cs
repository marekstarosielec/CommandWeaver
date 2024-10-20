namespace Models.Interfaces;

/// <summary>
/// Provides functionality for extracting variables or placeholders from strings
/// based on specific delimiters, such as double braces.
/// </summary>
/// <remarks>
/// The service is designed to support extraction of variables defined between 
/// various types of delimiters in the future, making it adaptable for multiple use cases.
/// </remarks>
public interface IVariableExtractionService
{
    /// <summary>
    /// Extracts the string between specified delimiters within a given input string.
    /// </summary>
    /// <param name="input">The input string from which to extract the variable.</param>
    /// <returns>
    /// The extracted string between the delimiters, or null if the delimiters are not found.
    /// </returns>
    /// <remarks>
    /// This method currently looks for the delimiters "{{" and "}}", but the delimiters may be customized in future versions.
    /// </remarks>
    string? ExtractVariableBetweenDelimiters(string input);

    /// <summary>
    /// Replaces all occurrences of a variable placeholder between delimiters with the provided value.
    /// </summary>
    /// <param name="input">The input string that contains placeholders.</param>
    /// <param name="variableName">The name of the variable to replace in the placeholders.</param>
    /// <param name="variableValue">The value to replace the variable placeholder with.</param>
    /// <returns>
    /// The input string with all instances of the variable placeholder replaced with the variable value.
    /// </returns>
    /// <remarks>
    /// This method searches for the pattern "{{ variableName }}" allowing for any amount of whitespace around the variable name.
    /// </remarks>
    string ReplaceVariableInString(string input, string variableName, string? variableValue);
}
