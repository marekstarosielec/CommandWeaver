// using System.Reflection;
//
// public class DynamicValueTests
// {
//     [Fact]
//     public void Constructor_WithTextValue_ShouldSetTextValue()
//     {
//         var dynamicValue = new DynamicValue("test");
//
//         Assert.Equal("test", dynamicValue.TextValue);
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void Constructor_WithDateTimeOffset_ShouldSetDateTimeValue()
//     {
//         var dateTime = DateTimeOffset.Now;
//         var dynamicValue = new DynamicValue(dateTime);
//
//         Assert.Equal(dateTime, dynamicValue.DateTimeValue);
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void Constructor_WithBoolean_ShouldSetBoolValue()
//     {
//         var dynamicValue = new DynamicValue(true);
//
//         Assert.True(dynamicValue.BoolValue);
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void Constructor_WithNumericValue_ShouldSetNumericValue()
//     {
//         var dynamicValue = new DynamicValue(123L);
//
//         Assert.Equal(123L, dynamicValue.NumericValue);
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void Constructor_WithPrecisionValue_ShouldSetPrecisionValue()
//     {
//         var dynamicValue = new DynamicValue(123.45);
//
//         Assert.Equal(123.45, dynamicValue.PrecisionValue);
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void Constructor_WithObjectValue_ShouldSetObjectValue()
//     {
//         var objectValue = new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } };
//         var dynamicValue = new DynamicValue(objectValue);
//
//         Assert.Equal(objectValue["key"]?.TextValue, dynamicValue.ObjectValue?["key"].TextValue);
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void Constructor_WithListValue_ShouldSetListValue()
//     {
//         var list = new List<DynamicValueObject> { new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } }) };
//         var dynamicValue = new DynamicValue(list);
//
//         Assert.Equal(list, dynamicValue.ListValue?.ToList());
//         Assert.True(dynamicValue.IsNull() == false);
//     }
//
//     [Fact]
//     public void IsNull_ShouldReturnTrue_WhenNoValuesAreSet()
//     {
//         var dynamicValue = new DynamicValue();
//
//         Assert.True(dynamicValue.IsNull());
//     }
//
//     [Fact]
//     public void IsNull_ShouldReturnFalse_WhenAnyValueIsSet()
//     {
//         var dynamicValue = new DynamicValue("test");
//
//         Assert.False(dynamicValue.IsNull());
//     }
//
//     [Fact]
//     public void GetEnumValue_ShouldReturnEnum_WhenValidEnumStringIsProvided()
//     {
//         var dynamicValue = new DynamicValue("Monday");
//
//         DayOfWeek? result = dynamicValue.GetEnumValue<DayOfWeek>();
//
//         Assert.Equal(DayOfWeek.Monday, result);
//     }
//
//     [Fact]
//     public void GetEnumValue_ShouldReturnNull_WhenInvalidEnumStringIsProvided()
//     {
//         var dynamicValue = new DynamicValue("InvalidDay");
//
//         DayOfWeek? result = dynamicValue.GetEnumValue<DayOfWeek>();
//
//         Assert.Null(result);
//     }
//
//     [Fact]
//     public void IsNull_ShouldIncludeAllProperties()
//     {
//         // Use reflection to get all public properties of the DynamicValue class
//         var publicProperties = typeof(DynamicValue)
//             .GetProperties(BindingFlags.Public | BindingFlags.Instance)
//             .Where(prop => prop.PropertyType.IsClass || Nullable.GetUnderlyingType(prop.PropertyType) != null);
//
//         // Assert that IsNull() returns false when each property is set individually
//         foreach (var property in publicProperties)
//         {
//             // Create a new instance with only this property set to a non-null value
//             var testInstance = CreateDynamicValueWithPropertySet(property);
//
//             // Assert that IsNull() returns false when the property is set
//             Assert.False(testInstance.IsNull(), $"IsNull() should return false when {property.Name} is set.");
//         }
//     }
//
//     [Fact]
//     public void DebuggerDisplay_ShouldIncludeAllProperties()
//     {
//         // Test each property individually and verify DebuggerDisplay output
//         AssertDebuggerDisplayForProperty(new DynamicValue("TestText"), "Text: \"TestText\"");
//         AssertDebuggerDisplayForProperty(new DynamicValue(DateTimeOffset.Now), $"DateTime: {DateTimeOffset.Now}");
//         AssertDebuggerDisplayForProperty(new DynamicValue(true), "Bool: True");
//         AssertDebuggerDisplayForProperty(new DynamicValue(123L), "Numeric: 123");
//         AssertDebuggerDisplayForProperty(new DynamicValue(123.45), "Precision: 123.45");
//
//         var obj = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("testValue") } });
//         AssertDebuggerDisplayForProperty(new DynamicValue(obj), $"Object: {obj["key"].TextValue}");
//
//         var list = new DynamicValueList();
//         AssertDebuggerDisplayForProperty(new DynamicValue(list), "List (Count: 0)");
//     }
//
//     private void AssertDebuggerDisplayForProperty(DynamicValue dynamicValue, string expectedDisplay)
//     {
//         // Access the private DebuggerDisplay property using reflection
//         var debuggerDisplay = typeof(DynamicValue)
//             .GetProperty("DebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance)?
//             .GetValue(dynamicValue)?.ToString();
//
//         Assert.Equal(expectedDisplay, debuggerDisplay);
//     }
//
// /// <summary>
// /// Helper method to create a new instance of DynamicValue with a specific property set to a non-null value.
// /// </summary>
// private DynamicValue CreateDynamicValueWithPropertySet(PropertyInfo property)
// {
//     // Determine the non-nullable test value for each property type
//     object testValue = property.PropertyType switch
//     {
//         Type type when type == typeof(string) => "test",
//         Type type when type == typeof(DateTimeOffset?) => DateTimeOffset.Now,
//         Type type when type == typeof(bool?) => true,
//         Type type when type == typeof(long?) => 123L,
//         Type type when type == typeof(double?) => 123.45,
//         Type type when type == typeof(DynamicValueObject) => new DynamicValueObject(new Dictionary<string, DynamicValue?>()),
//         Type type when type == typeof(DynamicValueList) => new DynamicValueList(new List<DynamicValueObject>()),
//         _ => throw new InvalidOperationException($"Unsupported property type: {property.PropertyType}")
//     };
//
//     var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
//
//     // Find the appropriate constructor for the property type and invoke it with the test value
//     var constructor = typeof(DynamicValue).GetConstructor(new[] { propertyType });
//     if (constructor != null)
//     {
//         return (DynamicValue)constructor.Invoke(new[] { testValue });
//     }
//
//     throw new InvalidOperationException($"No suitable constructor found for property {property.Name} of type {property.PropertyType}");
// }
// }
