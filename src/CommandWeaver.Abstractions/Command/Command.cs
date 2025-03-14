using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a command consisting of a name, a list of operations to be executed, and a set of parameters.
/// </summary>
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record Command
{
    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    /// <remarks>This is a required field and uniquely identifies the command.</remarks>
    public required DynamicValue Name { get; init; }

    /// <summary>
    /// Gets or sets the description of the command.
    /// </summary>
    /// <remarks>
    /// The description provides additional information about the purpose or usage of the command.
    /// </remarks>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Command category. Allows to build help list grouped into categories.
    /// </summary>
    public string? Category { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of operations associated with the command.
    /// </summary>
    /// <remarks>
    /// Each operation in this list defines a specific action or series of actions to be executed as part of the command.
    /// </remarks>
    public List<DynamicValue> Operations { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of parameters associated with the command.
    /// </summary>
    /// <remarks>
    /// These parameters provide additional context or configuration for the command and its operations.
    /// </remarks>
    public List<DynamicValue> Parameters { get; set; } = [];
 
    /// <summary>
    /// Contains json definition of command. Filled by to CommandConverter.
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Contains command deserialize as DynamicValue, so it can be accessed in operations. Filled by to CommandConverter.
    /// </summary>
    public DynamicValue Definition { get; set; } = new ();

    public List<string> GetAllNames()
    {
        var result = new List<string>();
        if (!string.IsNullOrWhiteSpace(Name.TextValue)) 
            result.Add(Name.TextValue);
        if (Name.ListValue == null) return result;
        
        foreach (var otherName in Name.ListValue)
            if (!string.IsNullOrWhiteSpace(otherName.TextValue))
                result.Add(otherName.TextValue);
        return result;
    }
}