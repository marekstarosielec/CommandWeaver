using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>
/// Represents a JSON object as an immutable dictionary, where keys are strings and values are dynamic. 
/// This class allows accessing and manipulating JSON-like data with dictionary-like behavior, providing 
/// safety through immutability while preserving flexibility in handling dynamic structures.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record DynamicValueObject
{
    private readonly ImmutableDictionary<string, DynamicValue?> _data = ImmutableDictionary<string, DynamicValue?>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValueObject"/> class with the specified dictionary.
    /// </summary>
    /// <param name="dictionary">A dictionary of string keys and <see cref="DynamicValue"/> values to initialize the object.</param>
    public DynamicValueObject(IDictionary<string, DynamicValue?> dictionary) => _data =
        dictionary as ImmutableDictionary<string, DynamicValue?> ?? dictionary.ToImmutableDictionary();

    /// <summary>
    /// Gets the <see cref="DynamicValue"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The <see cref="DynamicValue"/> associated with the specified key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the key does not exist in the dictionary.</exception>
    public DynamicValue this[string key] => _data.TryGetValue(key, out var value)
        ? value ?? throw new ArgumentOutOfRangeException(nameof(key), $"The key '{key}' exists but its value is null.")
        : throw new ArgumentOutOfRangeException(nameof(key), $"The key '{key}' does not exist in the dictionary.");

    /// <summary>
    /// Gets the collection of keys in the dictionary.
    /// </summary>
    public IEnumerable<string> Keys => _data.Keys;

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The <see cref="DynamicValue"/> associated with the key, or <c>null</c> if the key is not found.</returns>
    public DynamicValue? GetValueOrDefault(string key) =>
        CollectionExtensions.GetValueOrDefault(_data, key);

    /// <summary>
    /// Provides a string representation of the object for debugging purposes.
    /// </summary>
    private string DebuggerDisplay =>
        $"Object with {Keys.Count()} keys: [{string.Join(", ", Keys.Take(5))}{(Keys.Count() > 5 ? ", ..." : "")}]";
}
