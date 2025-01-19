using System.Collections.Immutable;

/// <summary>
/// Interface applied to CommandParameter and OperationParameter, which defines options of validity check.
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Gets or sets a value indicating whether this command parameter is required.
    /// </summary>
    /// <remarks>
    /// If <c>true</c>, the parameter must be provided in the command line arguments; otherwise, it is optional.
    /// </remarks>
    bool Required { get; }
    
    /// <summary>
    /// Gets the list of allowed text values.
    /// </summary>
    /// <remarks>
    /// If specified, only these values are accepted for the parameter. Values outside this list will result 
    /// in validation errors during parsing.
    /// </remarks>
    ImmutableList<string>? AllowedTextValues { get; }
    
    /// <summary>
    /// Gets the allowed enum type for this parameter.
    /// </summary>
    /// <remarks>
    /// If specified, the parameter value is restricted to valid values of this enum type. This enforces
    /// type-safe, predefined options for the parameter.
    /// </remarks>
    Type? AllowedEnumValues { get; }
    
    /// <summary>
    /// Indicates what type of data is allowed: text. Other types like date, number, etc. will be added later.
    /// </summary>
    string? AllowedType { get; }
    
    /// <summary>
    /// Indicates whether list should be used. <c>true</c> - only lists are allowed. <c>false</c> - lists are not allowed. 
    /// </summary>
    bool? List { get; }
}