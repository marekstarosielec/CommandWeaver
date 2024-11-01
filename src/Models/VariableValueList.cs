using System.Collections;
using System.Collections.Immutable;

namespace Models;

public record VariableValueList : IEnumerable<VariableValueObject>
{
    private ImmutableList<VariableValueObject> _items = ImmutableList<VariableValueObject>.Empty;


    public VariableValueList() { }

    public VariableValueList(IList<VariableValueObject> items) => _items = items.ToImmutableList();

    
    // Indexer to access list elements
    public VariableValueObject this[int index] => _items[index];

    
    // Method to add items to the list (returns a new record with updated list)
    public VariableValueList Add(VariableValueObject item) => this with { _items = _items.Add(item) };

    // Method to find the first element or default value
    public VariableValueObject? FirstOrDefault(Func<VariableValueObject, bool> predicate) => _items.FirstOrDefault(predicate);

    // Method to find the first element or default value
    public VariableValueObject? FirstOrDefault() => _items.FirstOrDefault();

    // Method to remove all matching elements
    public VariableValueList RemoveAll(Predicate<VariableValueObject> match) => this with { _items = _items.RemoveAll(match) };


    // Implementing GetEnumerator to support foreach
    public IEnumerator<VariableValueObject> GetEnumerator() => _items.GetEnumerator();

    // Explicit implementation for IEnumerable
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
