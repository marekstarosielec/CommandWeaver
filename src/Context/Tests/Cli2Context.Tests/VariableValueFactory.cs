using Models;
using System.Collections;

namespace Cli2Context.Tests;

internal class VariableValueFactory
{
    public DynamicValue SingleTextValue(string value) => new DynamicValue(value);

    public VariableValueFactoryObject Object() => new VariableValueFactoryObject();

    public VariableValueFactoryList List() => new VariableValueFactoryList();
}

internal class VariableValueFactoryObject
{
    private Dictionary<string, DynamicValue?> _properties = new ();

    public VariableValueFactoryObject AddTextProperty(string name, string value)
    {
        _properties[name] = new DynamicValue(value);
        return this;
    }

    public DynamicValue Build()
    {
        var objectValue = new Dictionary<string, DynamicValue?>();
        foreach (var property in _properties)
            objectValue[property.Key] = property.Value;
        return new DynamicValue(objectValue);
    }
}

internal class VariableValueFactoryList
{
    private Dictionary<string, Dictionary<string, DynamicValue?>> _list = new ();

    public VariableValueFactoryList AddElementWithTextProperty(string key, string propertyName, string propertyValue)
    {
        _list[key] = new Dictionary<string, DynamicValue?> { { propertyName, new DynamicValue(propertyValue) } };
        return this;
    }

    public DynamicValue Build()
    {
        var listValue = new List<DynamicValueObject>();
        foreach (var element in _list)
        {
            var objectValue = new Dictionary<string, DynamicValue?>();
            objectValue["key"] = new DynamicValue(element.Key);
            foreach (var property in element.Value)
                objectValue[property.Key] = property.Value;
            listValue.Add(new DynamicValueObject(objectValue));
        }
        return new DynamicValue(listValue);
    }
}