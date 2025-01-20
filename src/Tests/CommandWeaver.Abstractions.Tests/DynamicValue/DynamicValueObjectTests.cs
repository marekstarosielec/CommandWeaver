using System.Collections.Immutable;

public class DynamicValueObjectTests
{
    [Fact]
    public void Constructor_WithImmutableDictionary_ShouldInitializeCorrectly()
    {
        // Arrange
        var immutableDictionary = ImmutableDictionary<string, DynamicValue?>.Empty
            .Add("key1", new DynamicValue("value1"))
            .Add("key2", new DynamicValue(42));

        // Act
        var dynamicValueObject = new DynamicValueObject(immutableDictionary);

        // Assert
        Assert.Equal(2, dynamicValueObject.Keys.Count());
        Assert.True(dynamicValueObject.ContainsKey("key1"));
        Assert.True(dynamicValueObject.ContainsKey("key2"));
        Assert.Equal("value1", dynamicValueObject["key1"].TextValue);
        Assert.Equal(42, dynamicValueObject["key2"].NumericValue);
    }

    [Fact]
    public void Constructor_WithDictionary_ShouldConvertToImmutableDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key1", new DynamicValue("value1") },
            { "key2", new DynamicValue(42) }
        };

        // Act
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Assert
        Assert.Equal(2, dynamicValueObject.Keys.Count());
        Assert.True(dynamicValueObject.ContainsKey("key1"));
        Assert.True(dynamicValueObject.ContainsKey("key2"));
        Assert.Equal("value1", dynamicValueObject["key1"].TextValue);
        Assert.Equal(42, dynamicValueObject["key2"].NumericValue);
    }

    [Fact]
    public void Constructor_WithEmptyDictionary_ShouldInitializeEmpty()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>();

        // Act
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Assert
        Assert.Empty(dynamicValueObject.Keys);
    }

    [Fact]
    public void Indexer_WithExistingKey_ShouldReturnCorrectValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue("value") }
        };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act
        var value = dynamicValueObject["key"];

        // Assert
        Assert.Equal("value", value.TextValue);
    }

    [Fact]
    public void Indexer_WithNonExistingKey_ShouldThrowException()
    {
        // Arrange
        var dynamicValueObject = new DynamicValueObject(new Dictionary<string, DynamicValue?>());

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dynamicValueObject["missingKey"]);
        Assert.Contains("missingKey", exception.Message);
    }

    [Fact]
    public void Indexer_WithNullValue_ShouldThrowException()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key", null }
        };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dynamicValueObject["key"]);
        Assert.Contains("exists but its value is null", exception.Message);
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue("value") }
        };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act
        var result = dynamicValueObject.ContainsKey("key");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var dynamicValueObject = new DynamicValueObject(new Dictionary<string, DynamicValue?>());

        // Act
        var result = dynamicValueObject.ContainsKey("missingKey");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValueOrDefault_WithExistingKey_ShouldReturnCorrectValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue("value") }
        };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act
        var value = dynamicValueObject.GetValueOrDefault("key");

        // Assert
        Assert.NotNull(value);
        Assert.Equal("value", value.TextValue);
    }

    [Fact]
    public void GetValueOrDefault_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var dynamicValueObject = new DynamicValueObject(new Dictionary<string, DynamicValue?>());

        // Act
        var value = dynamicValueObject.GetValueOrDefault("missingKey");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void Keys_ShouldReturnAllKeys()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key1", new DynamicValue("value1") },
            { "key2", new DynamicValue("value2") }
        };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act
        var keys = dynamicValueObject.Keys.ToList();

        // Assert
        Assert.Contains("key1", keys);
        Assert.Contains("key2", keys);
    }
}
