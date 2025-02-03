using NSubstitute;

public class JsonSerializerTests
{
    private readonly JsonSerializer _serializer;

    public JsonSerializerTests()
    {
        var dynamicValueConverter = Substitute.For<IDynamicValueConverter>();
        var commandConverter = Substitute.For<ICommandConverter>();
        _serializer = new JsonSerializer(dynamicValueConverter, commandConverter);
    }
    
    [Fact]
    public void TryDeserialize_ShouldReturnFalse_WhenInvalidJsonIsProvided()
    {
        // Arrange
        var json = @"{ ""operation"": "; // Malformed JSON

        // Act
        var result = _serializer.TryDeserialize<Operation>(json, out var operation, out var exception);

        // Assert
        Assert.False(result);
        Assert.Null(operation);
        Assert.NotNull(exception);
    }

    [Fact]
    public void TrySerialize_ShouldReturnTrue_WhenSerializationIsSuccessful()
    {
        // Arrange
        var value = new { operation = "operation" };

        // Act
        var result = _serializer.TrySerialize(value, out var json, out var exception);

        // Assert
        Assert.True(result);
        Assert.Null(exception);
        Assert.NotNull(json);
        Assert.Contains("\"operation\"", json, StringComparison.OrdinalIgnoreCase);
    }
}
