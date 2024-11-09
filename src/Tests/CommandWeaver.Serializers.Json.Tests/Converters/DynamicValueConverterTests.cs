using System.Reflection;
using System.Text.Json;
using Models;

public class DynamicValueConverterTests
{
    private readonly JsonSerializerOptions _options;
    private readonly DynamicValueConverter _converter;

    public DynamicValueConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new ConverterWrapper<DynamicValue?>(new DynamicValueConverter()) }
        };
        _converter = new DynamicValueConverter();
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Read_BooleanValue_ShouldReturnCorrectDynamicValue(string json, bool expected)
    {
        var dynamicValue = System.Text.Json.JsonSerializer.Deserialize<DynamicValue>(json, _options);
        Assert.NotNull(dynamicValue);
        Assert.Equal(expected, dynamicValue?.BoolValue);
    }

    [Theory]
    [InlineData("12345", 12345L)]
    [InlineData("-987654321", -987654321L)]
    public void Read_NumberValue_Long_ShouldReturnCorrectDynamicValue(string json, long expected)
    {
        var dynamicValue = System.Text.Json.JsonSerializer.Deserialize<DynamicValue>(json, _options);
        Assert.NotNull(dynamicValue);
        Assert.Equal(expected, dynamicValue?.NumericValue);
    }

    [Theory]
    [InlineData("123.456", 123.456)]
    [InlineData("-98765.4321", -98765.4321)]
    public void Read_NumberValue_Double_ShouldReturnCorrectDynamicValue(string json, double expected)
    {
        var dynamicValue = System.Text.Json.JsonSerializer.Deserialize<DynamicValue>(json, _options);
        Assert.NotNull(dynamicValue);
        Assert.Equal(expected, dynamicValue?.PrecisionValue);
    }

    [Theory]
    [InlineData("\"2024-11-09T15:30:00Z\"", "2024-11-09T15:30:00Z")]
    [InlineData("\"2022-06-15T12:00:00-05:00\"", "2022-06-15T12:00:00-05:00")]
    public void Read_StringValue_DateTime_ShouldReturnCorrectDynamicValue(string json, string dateTimeString)
    {
        var expected = DateTimeOffset.Parse(dateTimeString);
        var dynamicValue = System.Text.Json.JsonSerializer.Deserialize<DynamicValue>(json, _options);
        Assert.NotNull(dynamicValue);
        Assert.Equal(expected, dynamicValue?.DateTimeValue);
    }

    [Fact]
    public void Read_ObjectValue_ShouldReturnCorrectDynamicValue()
    {
        string json = "{\"key1\": \"value1\", \"key2\": 123}";
        var dynamicValue = System.Text.Json.JsonSerializer.Deserialize<DynamicValue>(json, _options);
        Assert.NotNull(dynamicValue);
        Assert.NotNull(dynamicValue?.ObjectValue);
        Assert.Equal("value1", dynamicValue?.ObjectValue?["key1"]?.TextValue);
        Assert.Equal(123, dynamicValue?.ObjectValue?["key2"]?.NumericValue);
    }

    [Fact]
    public void Read_ArrayValue_ShouldReturnCorrectDynamicValue()
    {
        string json = "[\"value1\", 123, true]";
        var dynamicValue = System.Text.Json.JsonSerializer.Deserialize<DynamicValue>(json, _options);
        Assert.NotNull(dynamicValue);
        Assert.NotNull(dynamicValue?.ListValue);
        Assert.Equal("value1", dynamicValue?.ListValue?[0]["key"]?.TextValue);
        Assert.Equal(123, dynamicValue?.ListValue?[1]["key"]?.NumericValue);
        Assert.True(dynamicValue?.ListValue?[2]["key"]?.BoolValue);
    }

    [Fact]
    public void Write_DynamicValue_ShouldSerializeCorrectly()
    {
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "stringValue", new DynamicValue("test") },
            { "boolValue", new DynamicValue(true) },
            { "numericValue", new DynamicValue(42L) },
            { "precisionValue", new DynamicValue(3.14) },
            { "dateTimeValue", new DynamicValue(DateTimeOffset.UtcNow) }
        });

        string json = System.Text.Json.JsonSerializer.Serialize(dynamicValue, _options);

        Assert.Contains("\"stringValue\":\"test\"", json);
        Assert.Contains("\"boolValue\":true", json);
        Assert.Contains("\"numericValue\":42", json);
        Assert.Contains("\"precisionValue\":3.14", json);
        Assert.Contains("\"dateTimeValue\"", json); // Ensure dateTime is serialized (exact time may vary)
    }

    [Fact]
    public void Write_ArrayValue_ShouldSerializeCorrectly()
    {
        var list = new List<DynamicValueObject>
        {
            new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value1") } }),
            new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue(123) } }),
            new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue(true) } })
        };
        var dynamicValue = new DynamicValue(list);
        string json = System.Text.Json.JsonSerializer.Serialize(dynamicValue, _options);

        Assert.Contains("\"key\":\"value1\"", json);
        Assert.Contains("\"key\":123", json);
        Assert.Contains("\"key\":true", json);
    }

    [Fact]
    public void Converter_CoversAllDynamicValueProperties()
    {
        // Get all properties from DynamicValue class
        var dynamicValueProperties = typeof(DynamicValue).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Define which properties are currently handled in the converter
        var handledProperties = new[]
        {
            nameof(DynamicValue.TextValue),
            nameof(DynamicValue.DateTimeValue),
            nameof(DynamicValue.BoolValue),
            nameof(DynamicValue.NumericValue),
            nameof(DynamicValue.PrecisionValue),
            nameof(DynamicValue.ObjectValue),
            nameof(DynamicValue.ListValue)
        };

        // Check that all properties in DynamicValue are covered by the converter
        foreach (var property in dynamicValueProperties)
            Assert.Contains(property.Name, handledProperties);
    }
}