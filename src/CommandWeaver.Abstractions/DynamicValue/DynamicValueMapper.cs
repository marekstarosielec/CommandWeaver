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
        var result = (T?)Map(dynamicValue, typeof(T));
        
        return result;
    }

    private static bool TypeIsEnumerable(Type type)
        => type.IsAssignableTo(typeof(IEnumerable)) && type != typeof(string);
    
    private static Type GetGenericCollectionType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        var interfaces= type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                               
        if (interfaces != null)
            return interfaces.GetGenericArguments()[0];

        throw new InvalidOperationException($"Type '{type.FullName}' does not implement IEnumerable<> or IEnumerable<>.");
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
    private static dynamic CreateInstance(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
            throw new InvalidOperationException($"Type {type.Name} does not have a parameterless constructor.");
        
        if (type.IsInterface || type.IsAbstract)
            throw new Exception("Cannot create instance of type " + type.FullName);
        
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
        
        var result = Activator.CreateInstance(type);
        if (result == null)
            throw new InvalidOperationException($"Failed to create instance of {type.FullName}");

        return result;
    }
    
    private static dynamic? Map(DynamicValue dynamicValue, Type targetType)
    {
        if (targetType == typeof(DynamicValue))
            return MapDynamicValue(dynamicValue);
        if (TypeIsObject(targetType) && dynamicValue.ObjectValue != null)
            return MapObject(dynamicValue.ObjectValue, targetType);
        if (TypeIsEnumerable(targetType) && dynamicValue.ListValue != null)
            return MapList(dynamicValue.ListValue, targetType);
        if (TypeIsEnumerable(targetType))
            return MapPrimitiveIntoList(dynamicValue, targetType);
        return MapPrimitive(dynamicValue, targetType); 
    }

    private static dynamic MapDynamicValue(DynamicValue dynamicValue) => dynamicValue;

    private static dynamic? MapPrimitive(DynamicValue dynamicValue, Type targetType)
    {
        if (targetType == typeof(string))
            return dynamicValue.TextValue;
        if (targetType == typeof(int) || targetType == typeof(int?))
        {
            var value = dynamicValue.NumericValue;
            if (value is >= int.MinValue and <= int.MaxValue)
                return (int) value.Value;
            throw new InvalidOperationException($"Cannot convert {dynamicValue.NumericValue} to int.");
        }
        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            return dynamicValue.DateTimeValue?.DateTime;
        if (targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?))
            return dynamicValue.DateTimeValue;
        if (targetType == typeof(long) || targetType == typeof(long?))
            return dynamicValue.NumericValue;
        if (targetType == typeof(double) || targetType == typeof(double?))
            return dynamicValue.PrecisionValue;
        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
        {
            var value = dynamicValue.PrecisionValue;
            if (value is >= (double)decimal.MinValue and <= (double)decimal.MaxValue)
                return (decimal) value.Value;
            throw new InvalidOperationException($"Cannot convert {dynamicValue.PrecisionValue} to decimal.");
        }
        if (targetType == typeof(bool) || targetType == typeof(bool?))
            return dynamicValue.BoolValue;

        return null;
    }

    private static dynamic MapPrimitiveIntoList(DynamicValue dynamicValue, Type targetType)
    {
        var result = CreateInstance(targetType);
        var listType = GetGenericCollectionType(targetType);
        result.Add(MapPrimitive(dynamicValue, listType));
        return result;
    }
    
    private static dynamic MapList(DynamicValueList dynamicValueList, Type targetType)
    {
        var result = CreateInstance(targetType);
        var listType = GetGenericCollectionType(targetType);
        foreach (var listItem in dynamicValueList)
        {
            var resultElement = Map(listItem, listType);
            result.Add(resultElement);
        }
        return result;
    }
    
    private static dynamic MapObject(DynamicValueObject dynamicValueObject,Type targetType)
    {
        var result = CreateInstance(targetType);
        foreach (var key in dynamicValueObject.Keys)
        {
            var property = GetProperty(targetType, key);
            if (property == null || !property.CanWrite) 
                throw new InvalidOperationException(
                    $"Property {key} does not exist or is not writable in type {targetType.Name}");
           
            var propertyValue = Map(dynamicValueObject[key], property.PropertyType);
            property.SetValue(result, propertyValue);
        }

        return result;
    }
}
