using System.Text.Json;

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
}
