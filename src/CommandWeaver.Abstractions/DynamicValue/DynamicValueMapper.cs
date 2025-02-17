using System;
using System.Linq;
using System.Reflection;
using System.Collections;

internal static class DynamicValueMapper
{
    /// <summary>
    /// Maps the values from a <see cref="DynamicValue"/> to a new instance of the specified type <typeparamref name="T"/>.
    /// Supports deep property mapping for nested objects.
    /// </summary>
    /// <typeparam name="T">The target type to map the values to.</typeparam>
    /// <param name="dynamicValue">The <see cref="DynamicValue"/> containing the values to map.</param>
    /// <returns>A new instance of type <typeparamref name="T"/> populated with the mapped values.</returns>
    public static T? MapTo<T>(DynamicValue dynamicValue) where T : new()
    {
        if (dynamicValue.ObjectValue == null)
            return default;

        var target = new T();
        MapObject(dynamicValue.ObjectValue, target, typeof(T));
        return target;
    }

    private static void MapObject(DynamicValueObject dynamicObject, object target, Type targetType)
    {
        // Ensure all keys in dynamicObject match a property in the target type
        if (targetType != typeof(DynamicValue))
        {
            var targetProperties = targetType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                .ToDictionary(p => p.Name.ToLower(), p => p);

            var invalidKeys = dynamicObject.Keys.Where(key => !targetProperties.ContainsKey(key.ToLower())).ToList();
            if (invalidKeys.Any())
                throw new InvalidOperationException(
                    $"Invalid properties found in DynamicValue for target type {targetType.Name}: {string.Join(", ", invalidKeys)}");
        }
        

        foreach (var key in dynamicObject.Keys)
        {
            var property = targetType.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null || !property.CanWrite) continue;

            var value = dynamicObject.GetValueOrDefault(key);
            if (value == null) continue;

            if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime))
            {
                // Map simple types directly
                property.SetValue(target, Convert.ChangeType(value.GetTextValue(), property.PropertyType));
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                // Handle nested objects
                if (value.ObjectValue != null)
                {
                    var nestedInstance = Activator.CreateInstance(property.PropertyType);
                    if (nestedInstance == null) continue;
                    MapObject(value.ObjectValue, nestedInstance, property.PropertyType);
                    property.SetValue(target, nestedInstance);
                }
                // Handle lists
                else if (value.ListValue != null && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var listType = property.PropertyType.IsGenericType
                        ? property.PropertyType.GetGenericArguments()[0]
                        : property.PropertyType.GetElementType();

                    if (listType == null) continue;
                    
                    var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType)) as IList;
                    foreach (var item in value.ListValue)
                    {
                        object? mappedItem = null;

                        if (item.ObjectValue != null)
                        {
                            mappedItem = Activator.CreateInstance(listType);
                            if (listType == typeof(DynamicValue))
                                mappedItem = new DynamicValue(item.ObjectValue);
                            else if (mappedItem != null)
                                MapObject(item.ObjectValue, mappedItem, listType);
                        }
                        else
                            mappedItem = Convert.ChangeType(item.GetTextValue(), listType);

                        listInstance?.Add(mappedItem);
                    }
                    property.SetValue(target, listInstance);
                }
            }
        }
    }
}
