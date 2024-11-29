using System.Reflection;
using System.Text.Json;
using NSubstitute;

public class DynamicValueConverterTests
{
    private readonly DynamicValueConverter _converter = new();

    [Fact]
    public void Read_ShouldConvertTrueToDynamicValue()
    {
        // Arrange
        var json = "true";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.BoolValue);
    }

    [Fact]
    public void Read_ShouldConvertFalseToDynamicValue()
    {
        // Arrange
        var json = "false";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.BoolValue);
    }

    [Fact]
    public void Read_ShouldConvertNumberToDynamicValue()
    {
        // Arrange
        var json = "123.45";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123.45, result.PrecisionValue);
    }

    [Fact]
    public void Read_ShouldConvertStringToDynamicValue()
    {
        // Arrange
        var json = "\"Hello, World!\"";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello, World!", result.TextValue);
    }

    [Fact]
    public void Read_ShouldConvertNullToDynamicValue()
    {
        // Arrange
        var json = "null";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsNull());
    }

    [Fact]
    public void Read_ShouldConvertObjectToDynamicValue()
    {
        // Arrange
        var json = "{\"key1\":\"value1\",\"key2\":123}";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ObjectValue);
        Assert.Equal("value1", result.ObjectValue["key1"].TextValue);
        Assert.Equal(123, result.ObjectValue["key2"].NumericValue);
    }

    [Fact]
    public void Read_ShouldConvertArrayToDynamicValue()
    {
        // Arrange
        var json = "[true, \"text\", 42]";
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var result = _converter.Read(ref reader, typeof(DynamicValue), new JsonSerializerOptions());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ListValue);
        Assert.True(result.ListValue[0].BoolValue);
        Assert.Equal("text", result.ListValue[1].TextValue);
        Assert.Equal(42, result.ListValue[2].NumericValue);
    }

    [Fact]
    public void Write_ShouldSerializeDynamicValueToString()
    {
        // Arrange
        var dynamicValue = new DynamicValue("Hello, World!");
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, dynamicValue, options);
        writer.Flush();

        // Assert
        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("\"Hello, World!\"", result);
    }

    [Fact]
    public void Write_ShouldSerializeDynamicValueToNumber()
    {
        // Arrange
        var dynamicValue = new DynamicValue(123.45);
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, dynamicValue, options);
        writer.Flush();

        // Assert
        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("123.45", result);
    }

    [Fact]
    public void Write_ShouldSerializeDynamicValueToObject()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "key1", new DynamicValue("value1") },
            { "key2", new DynamicValue(123) }
        });
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, dynamicValue, options);
        writer.Flush();

        // Assert
        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(result);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Count);
        Assert.Equal("value1", Convert.ToString(deserialized["key1"]));
        Assert.Equal(123, int.Parse(Convert.ToString(deserialized["key2"])!));
    }

    [Fact]
    public void Write_ShouldSerializeDynamicValueToArray()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new List<DynamicValue>
        {
            new DynamicValue(true),
            new DynamicValue("text"),
            new DynamicValue(42)
        });
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, dynamicValue, options);
        writer.Flush();

        // Assert
        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("[true,\"text\",42]", result);
    }
    
    [Fact]
    public void Write_ShouldHandleAllDynamicValueProperties()
    {
         var properties = typeof(DynamicValue)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.Name == nameof(DynamicValue.NoResolving))
                continue;
            
            // Arrange
            var dynamicValue = new DynamicValue(); // A test instance of DynamicValue
            using var memoryStream = new MemoryStream();
            using var writer = new Utf8JsonWriter(memoryStream);
            var options = new JsonSerializerOptions();

            var dynamicValueConverter = new DynamicValueConverter();

            // Get the backing field for the property
            var backingField = typeof(DynamicValue)
                .GetField($"<{property.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

            if (backingField == null)
            {
                Assert.Fail($"Backing field for property '{property.Name}' not found.");
                continue;
            }

            // Get the test value for the property type
            var testValue = GetTestValueForProperty(property.PropertyType);
            if (testValue == null)
            {
                Assert.Fail($"No test value defined for property '{property.Name}' of type '{property.PropertyType}'.");
                continue;
            }

            // Act
            backingField.SetValue(dynamicValue, testValue);

            memoryStream.SetLength(0); // Reset the memory stream
            dynamicValueConverter.Write(writer, dynamicValue, options);
            writer.Flush();
            memoryStream.Position = 0;

            // Read the result
            var writtenJson = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            var expectedJson = GetExpectedJsonForTestValue(testValue);
            if (!writtenJson.Contains(expectedJson, StringComparison.Ordinal))
                Assert.Fail($"Unsupported DynamicValue type {property.Name}");

            // Reset the backing field value
            backingField.SetValue(dynamicValue, null);
        }
    }

    private static object? GetTestValueForProperty(Type propertyType)
    {
        // Provide test values for each property type
        if (propertyType == typeof(string)) return "TestString";
        if (propertyType == typeof(bool?)) return true;
        if (propertyType == typeof(DateTimeOffset?)) return DateTimeOffset.UtcNow;
        if (propertyType == typeof(long?)) return 123456789L;
        if (propertyType == typeof(double?)) return 12345.6789;
        if (propertyType == typeof(DynamicValueObject)) return new DynamicValueObject(new Dictionary<string, DynamicValue?>());
        if (propertyType == typeof(DynamicValueList)) return new DynamicValueList(new List<DynamicValue>());
        return null; // Return null for unsupported types
    }
    
    private static string GetExpectedJsonForTestValue(object testValue) =>
        // Generate expected JSON based on the type of the test value
        testValue switch
        {
            string str => $"\"{str}\"",
            bool boolVal => boolVal.ToString().ToLowerInvariant(),
            DateTimeOffset dto => $"\"{dto.ToUniversalTime():O}\"",
            long longVal => longVal.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double doubleVal => doubleVal.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DynamicValueObject => "{", // JSON object starts with '{'
            DynamicValueList => "[", // JSON array starts with '['
            _ => "null" // Unhandled types default to null
        };
}
