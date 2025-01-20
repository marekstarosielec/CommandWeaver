using System.Reflection;

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
        MapObject(dynamicValue.ObjectValue, target);
        return target;
    }

    private static void MapObject(DynamicValueObject dynamicObject, object target)
    {
        var targetType = target.GetType();

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
                    MapObject(value.ObjectValue, nestedInstance!);
                    property.SetValue(target, nestedInstance);
                }
            }
        }
    }
}