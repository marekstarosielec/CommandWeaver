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
    public static T? MapTo<T>(DynamicValue dynamicValue) 
    {
        // var target = CreateInstance(typeof(T));
        // if (target == null)
        //     throw new InvalidOperationException($"Failed to create instance of {typeof(T).FullName}");
        var targetWrapper = new TargetWrapper();
        
        Map(dynamicValue, targetWrapper, typeof(T));
        return (T?) targetWrapper.Target;
        
        // if (dynamicValue.ObjectValue == null)
        //     return default;
        // MapObjectOld(dynamicValue.ObjectValue, target, typeof(T));
        // return (T?) target;
    }
    
    // public static T? MapToNew<T>(DynamicValue dynamicValue) where T : new()
    // {
    //     if (dynamicValue.ObjectValue == null)
    //         return default;
    //
    //     var target = new T();
    //     //MapObject(dynamicValue.ObjectValue, target, typeof(T));
    //     Map(dynamicValue, target, typeof(T));
    //     return target;
    // }

    private static bool TypeIsEnumerable(Type type)
        => type.IsAssignableTo(typeof(IEnumerable)) && type != typeof(string);

    private static bool TypeIsOrImplementsEnumerableOf(Type targetType, Type type)
    {
        if (targetType == type)
            return true;

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return targetType.GetGenericArguments()[0] == type;

        return targetType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>) 
                                      && i.GetGenericArguments()[0] == type);
    }
    
    private static bool TypeIsObject(Type type)
        => type.IsClass && type != typeof(string);

    private static PropertyInfo? GetProperty(Type type, string propertyName) => type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);


    /// <summary>
    /// Create instance of given type. Supports lists and dictionaries.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    private static object? CreateInstance(Type type)
    {
        if (type == typeof(string))
            return string.Empty; // Default value for string
        
        if (type == typeof(int))
            return string.Empty; // Default value for string

        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
            throw new InvalidOperationException($"Type {type.FullName} does not have a parameterless constructor.");
        
        if (type.IsInterface || type.IsAbstract)
            throw new Exception("Cannot create instance of type " + type.FullName);
        
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
        
        var result = Activator.CreateInstance(type);
        if (result == null)
            throw new InvalidOperationException($"Failed to create instance of {type.FullName}");

        return result;
    }
    
    private static void Map(DynamicValue dynamicValue, TargetWrapper targetWrapper, Type targetType)
    {
        if (targetType == typeof(DynamicValue))
            MapDynamicValue(dynamicValue, targetWrapper, targetType);
        else if (TypeIsOrImplementsEnumerableOf(targetType, typeof(string)) && !TypeIsEnumerable(targetType))
            MapString(dynamicValue, targetWrapper, targetType);
        else if (TypeIsOrImplementsEnumerableOf(targetType, typeof(string)) && TypeIsEnumerable(targetType))
            MapStringIntoList(dynamicValue, targetWrapper, targetType);
        else if (dynamicValue.ObjectValue != null && TypeIsObject(targetType))
            MapObject(dynamicValue.ObjectValue, targetWrapper, targetType);
        else if (dynamicValue.ListValue != null && TypeIsEnumerable(targetType))
            MapList(dynamicValue.ListValue, targetWrapper, targetType);
        
    }

    private static void MapDynamicValue(DynamicValue dynamicValue, TargetWrapper targetWrapper, Type targetType)
    {
        targetWrapper.Target = dynamicValue;
    }
    
    private static void MapString(DynamicValue dynamicValue, TargetWrapper targetWrapper, Type targetType)
    {
        targetWrapper.Target = dynamicValue.TextValue;
    }
    
    private static void MapStringIntoList(DynamicValue dynamicValue, TargetWrapper targetWrapper, Type targetType)
    {
        if (targetWrapper.Target is not ICollection<string>)
            targetWrapper.Target = CreateInstance(targetType);
        ((ICollection<string>)targetWrapper.Target!).Add(dynamicValue.TextValue ?? string.Empty);
    }
    
    // private static void MapDateTime(DynamicValue dynamicValue, TargetWrapper targetWrapper, Type targetType)
    // {
    //     targetWrapper.Target = dynamicValue.TextValue;
    // }
    
    private static void MapList(DynamicValueList dynamicValueList, TargetWrapper targetWrapper, Type targetType)
    {
        
    }
    
    private static void MapObject(DynamicValueObject dynamicValueObject, TargetWrapper targetWrapper, Type targetType)
    {
        targetWrapper.Target = CreateInstance(targetType);
        foreach (var key in dynamicValueObject.Keys)
        {
            var property = GetProperty(targetType, key);
            if (property == null || !property.CanWrite) 
                throw new InvalidOperationException(
                    $"Property {key} does not exist or is not writable in type {targetType.Name}");
           
            var innerTargetWrapper = new TargetWrapper();
            Map(dynamicValueObject[key], innerTargetWrapper, property.PropertyType);
            property.SetValue(targetWrapper.Target, innerTargetWrapper.Target);
        }
    }

    private static void MapObjectOld(DynamicValueObject dynamicObject, object target, Type targetType)
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

            var propertyType = property.PropertyType;
            // if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            // {
            //     if (property.PropertyType.IsArray)
            //         // Property is an array type, get element type directly
            //         propertyType = property.PropertyType.GetElementType();
            //     else if (property.PropertyType.IsGenericType)
            //         // For IEnumerable<T> or IList<T>, get generic argument
            //         propertyType = property.PropertyType.GetGenericArguments()[0];
            //     else
            //         // Handle cases like non-generic IEnumerable
            //         propertyType = typeof(object);
            // }
            // if (propertyType == null)
            //     propertyType = property.PropertyType;
            
            if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(DateTime))
            {
                // if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                // {
                //     // Target is IEnumerable<T>, create list and add primitive value
                //     var listType = property.PropertyType.IsGenericType
                //         ? property.PropertyType.GetGenericArguments()[0]
                //         : property.PropertyType.GetElementType();
                //
                //     if (listType == null) continue;
                //
                //     //What if list is already created?
                //     var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType)) as IList;
                //     listInstance?.Add(Convert.ChangeType(value.GetTextValue(), listType));
                //
                //     property.SetValue(target, listInstance);
                // }
                // else
                    // Map simple types directly
                    property.SetValue(target, Convert.ChangeType(value.GetTextValue(), property.PropertyType));
            }
            else if (property.PropertyType == typeof(DynamicValue))
                property.SetValue(target, value);
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                // Handle nested objects
                if (value.ObjectValue != null)
                {
                    var nestedInstance = Activator.CreateInstance(property.PropertyType);
                    if (nestedInstance == null) continue;
                    MapObjectOld(value.ObjectValue, nestedInstance, property.PropertyType);
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
                                MapObjectOld(item.ObjectValue, mappedItem, listType);
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

    private class TargetWrapper
    {
        public object? Target { get; set; }
    }
}
