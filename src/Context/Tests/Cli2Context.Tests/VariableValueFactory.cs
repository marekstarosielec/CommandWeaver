using Models;

namespace Cli2Context.Tests;

internal class VariableValueFactory
{
    public DynamicValue SingleTextValue(string value) => new DynamicValue { TextValue = value };

    public VariableValueFactoryObject Object() => new VariableValueFactoryObject();

    public VariableValueFactoryList List() => new VariableValueFactoryList();
}

internal class VariableValueFactoryObject
{
    private Dictionary<string, DynamicValue?> _properties = new ();

    public VariableValueFactoryObject AddTextProperty(string name, string value)
    {
        _properties[name] = new DynamicValue { TextValue = value };
        return this;
    }

    public DynamicValue Build()
    {
        var objectValue = new DynamicValueObject();
        foreach (var property in _properties)
            objectValue = objectValue.With(property.Key, property.Value);
        return new DynamicValue { ObjectValue = objectValue };
    }
}

internal class VariableValueFactoryList
{
    private Dictionary<string, Dictionary<string, DynamicValue?>> _list = new ();

    public VariableValueFactoryList AddElementWithTextProperty(string key, string propertyName, string propertyValue)
    {
        _list[key] = new Dictionary<string, DynamicValue?> { { propertyName, new DynamicValue { TextValue = propertyValue } } };
        return this;
    }

    public DynamicValue Build()
    {
        var listValue = new DynamicValueList();
        foreach (var element in _list)
        {
            var objectValue = new DynamicValueObject();
            foreach (var property in element.Value)
                objectValue = objectValue.With(property.Key, property.Value);
            listValue = listValue.Add(objectValue.With("key", new DynamicValue { TextValue = element.Key }));
        }
        return new DynamicValue { ListValue = listValue };
    }
}