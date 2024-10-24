using System.Collections.Immutable;

namespace Models;

public record VariableValueList
{
    private ImmutableList<VariableValueObject> _items = ImmutableList<VariableValueObject>.Empty;

    // Constructor that creates empty object
    public VariableValueList() { }

    // Constructor that accepts a single text value
    public VariableValueList(string property, string? textValue) => _items = ImmutableList<VariableValueObject>.Empty.Add(new VariableValueObject(property, new VariableValue(textValue)));

    
    // Indexer to access list elements
    public VariableValueObject this[int index] => _items[index];

    // Property to expose the immutable list
    public ImmutableList<VariableValueObject> Items => _items;

    // Method to add items to the list (returns a new record with updated list)
    public VariableValueList Add(VariableValueObject item)
    {
        return this with { _items = _items.Add(item) };
    }

    // Method to find the first element or default value
    public VariableValueObject? FirstOrDefault(Func<VariableValueObject, bool> predicate) => _items.FirstOrDefault(predicate);

    // Method to find the first element or default value
    public VariableValueObject? FirstOrDefault() => _items.FirstOrDefault();
}
