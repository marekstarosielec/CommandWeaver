using Models;

namespace Cli2Context.Tests;

internal class VariableValueFactory
{
    public VariableValue SingleTextValue(string value) => new VariableValue { TextValue = value };

    public VariableValueFactoryObject Object() => new VariableValueFactoryObject();

    public VariableValueFactoryList List() => new VariableValueFactoryList();
}

internal class VariableValueFactoryObject
{
    private Dictionary<string, VariableValue?> _properties = new ();

    public VariableValueFactoryObject AddTextProperty(string name, string value)
    {
        _properties[name] = new VariableValue { TextValue = value };
        return this;
    }

    public VariableValue Build()
    {
        var objectValue = new VariableValueObject();
        foreach (var property in _properties)
            objectValue = objectValue.With(property.Key, property.Value);
        return new VariableValue { ObjectValue = objectValue };
    }
}

internal class VariableValueFactoryList
{
    private Dictionary<string, Dictionary<string, VariableValue?>> _list = new ();

    public VariableValueFactoryList AddElementWithTextProperty(string key, string propertyName, string propertyValue)
    {
        _list[key] = new Dictionary<string, VariableValue?> { { propertyName, new VariableValue { TextValue = propertyValue } } };
        return this;
    }

    public VariableValue Build()
    {
        var listValue = new VariableValueList();
        foreach (var element in _list)
        {
            var objectValue = new VariableValueObject();
            foreach (var property in element.Value)
                objectValue = objectValue.With(property.Key, property.Value);
            listValue = listValue.Add(objectValue.With("key", new VariableValue { TextValue = element.Key }));
        }
        return new VariableValue { ListValue = listValue };
    }
}