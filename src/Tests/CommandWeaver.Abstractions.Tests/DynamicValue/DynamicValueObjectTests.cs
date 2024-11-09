public class DynamicValueObjectTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithGivenDictionary()
    {
        // Arrange
        var initialData = new Dictionary<string, DynamicValue?>
        {
            { "key1", new DynamicValue("value1") },
            { "key2", new DynamicValue("value2") }
        };

        // Act
        var dynamicValueObject = new DynamicValueObject(initialData);

        // Assert
        Assert.Equal(2, dynamicValueObject.Keys.Count());
        Assert.Contains("key1", dynamicValueObject.Keys);
        Assert.Contains("key2", dynamicValueObject.Keys);
        Assert.Equal("value1", dynamicValueObject["key1"].TextValue);
        Assert.Equal("value2", dynamicValueObject["key2"].TextValue);
    }

    [Fact]
    public void Indexer_ShouldReturnDynamicValue_WhenKeyExists()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue("testValue") }
        };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act
        var result = dynamicValueObject["key"];

        // Assert
        Assert.Equal("testValue", result.TextValue);
    }

    [Fact]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenKeyDoesNotExist()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dynamicValueObject["nonexistentKey"]);
        Assert.Equal("Specified key does not exist. (Parameter 'key')", exception.Message);
    }

    [Fact]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenValueIsNull()
    {
        // Arrange
        var dictionary = new Dictionary<string, DynamicValue?> { { "keyWithNullValue", null } };
        var dynamicValueObject = new DynamicValueObject(dictionary);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dynamicValueObject["keyWithNullValue"]);
        Assert.Equal("Value for the specified key is null. (Parameter 'key')", exception.Message);
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
        var keys = dynamicValueObject.Keys;

        // Assert
        Assert.Contains("key1", keys);
        Assert.Contains("key2", keys);
        Assert.Equal(2, keys.Count());
    }

    [Fact]
    public void Keys_ShouldReturnEmptyCollection_WhenNoDataIsPresent()
    {
        // Arrange
        var dynamicValueObject = new DynamicValueObject(new Dictionary<string, DynamicValue?>());

        // Act
        var keys = dynamicValueObject.Keys;

        // Assert
        Assert.Empty(keys);
    }
}
