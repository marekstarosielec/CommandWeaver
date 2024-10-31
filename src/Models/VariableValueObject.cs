using System.Collections.Immutable;

namespace Models;

public record VariableValueObject
{
    private ImmutableDictionary<string, VariableValue?> _data = ImmutableDictionary<string, VariableValue?>.Empty;

    // Constructor that creates empty object
    public VariableValueObject() { }

    // Constructor that accepts a regular Dictionary
    public VariableValueObject(IDictionary<string, VariableValue?> dictionary) => _data = dictionary.ToImmutableDictionary();

    // Indexer for dictionary-like access
    public VariableValue this[string key] => _data[key] ?? throw new ArgumentOutOfRangeException(nameof(key));

    public IEnumerable<string> Keys { get => _data.Keys; }

    /// <summary>
    /// Creates a copy of object with modified element.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public VariableValueObject With(string key, VariableValue? value) => new VariableValueObject(_data.SetItem(key, value));

}