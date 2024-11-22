public class DynamicValueTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldInitializeWithNullValues()
    {
        // Act
        var dynamicValue = new DynamicValue();

        // Assert
        Assert.True(dynamicValue.IsNull());
    }

    [Fact]
    public void Constructor_WithTextValue_ShouldInitializeCorrectly()
    {
        // Act
        var dynamicValue = new DynamicValue("test");

        // Assert
        Assert.Equal("test", dynamicValue.TextValue);
        Assert.True(dynamicValue.IsNull() == false);
    }

    [Fact]
    public void Constructor_WithNoResolving_ShouldSetPropertyCorrectly()
    {
        // Act
        var dynamicValue = new DynamicValue("{{ name }}", true);

        // Assert
        Assert.Equal("{{ name }}", dynamicValue.TextValue);
        Assert.True(dynamicValue.NoResolving);
    }

    [Fact]
    public void Constructor_WithDateTime_ShouldInitializeCorrectly()
    {
        // Arrange
        var dateTime = DateTimeOffset.UtcNow;

        // Act
        var dynamicValue = new DynamicValue(dateTime);

        // Assert
        Assert.Equal(dateTime, dynamicValue.DateTimeValue);
    }

    [Fact]
    public void Constructor_WithBoolean_ShouldInitializeCorrectly()
    {
        // Act
        var dynamicValue = new DynamicValue(true);

        // Assert
        Assert.True(dynamicValue.BoolValue);
    }

    [Fact]
    public void Constructor_WithNumericValue_ShouldInitializeCorrectly()
    {
        // Act
        var dynamicValue = new DynamicValue(12345L);

        // Assert
        Assert.Equal(12345L, dynamicValue.NumericValue);
    }

    [Fact]
    public void Constructor_WithPrecisionValue_ShouldInitializeCorrectly()
    {
        // Act
        var dynamicValue = new DynamicValue(3.14);

        // Assert
        Assert.Equal(3.14, dynamicValue.PrecisionValue);
    }

    [Fact]
    public void Constructor_WithObjectValue_ShouldInitializeCorrectly()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue("value") }
        };

        // Act
        var dynamicValue = new DynamicValue(dictionary);

        // Assert
        Assert.NotNull(dynamicValue.ObjectValue);
        Assert.Equal("value", dynamicValue.ObjectValue["key"].TextValue);
    }

    [Fact]
    public void Constructor_WithListValue_ShouldInitializeCorrectly()
    {
        // Arrange
        var list = new List<DynamicValue>
        {
            new DynamicValue("value1"),
            new DynamicValue("value2")
        };

        // Act
        var dynamicValue = new DynamicValue(list);

        // Assert
        Assert.NotNull(dynamicValue.ListValue);
        Assert.Equal(2, dynamicValue.ListValue.Count);
        Assert.Equal("value1", dynamicValue.ListValue[0].TextValue);
        Assert.Equal("value2", dynamicValue.ListValue[1].TextValue);
    }

    [Fact]
    public void GetEnumValue_ValidEnumText_ShouldReturnEnumValue()
    {
        // Arrange
        var dynamicValue = new DynamicValue("Monday");

        // Act
        var result = dynamicValue.GetEnumValue<DayOfWeek>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DayOfWeek.Monday, result);
    }

    [Fact]
    public void GetEnumValue_InvalidEnumText_ShouldReturnNull()
    {
        // Arrange
        var dynamicValue = new DynamicValue("InvalidValue");

        // Act
        var result = dynamicValue.GetEnumValue<DayOfWeek>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsNull_WithNonNullProperties_ShouldReturnFalse()
    {
        // Act
        var dynamicValue = new DynamicValue("NonNull");

        // Assert
        Assert.False(dynamicValue.IsNull());
    }
}
