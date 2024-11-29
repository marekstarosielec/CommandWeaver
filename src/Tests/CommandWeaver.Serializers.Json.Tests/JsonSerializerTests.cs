using System.Text.Json;
using NSubstitute;

public class JsonSerializerTests
{
    private readonly IOperationConverter _operationConverter;
    private readonly IDynamicValueConverter _dynamicValueConverter;
    private readonly JsonSerializer _serializer;

    public JsonSerializerTests()
    {
        _operationConverter = Substitute.For<IOperationConverter>();
        _dynamicValueConverter = Substitute.For<IDynamicValueConverter>();
        _serializer = new JsonSerializer(_operationConverter, _dynamicValueConverter);
    }

    [Fact]
    public void TryDeserialize_ShouldReturnTrue_WhenValidJsonIsProvided()
    {
        // Arrange
        var json = @"{ ""operation"": ""TestOperation"", ""parameters"": {} }";
        var testOperation = Substitute.For<Operation>();
        var testOperationConverter = new TestOperationConverter(testOperation);

        var dynamicValueConverter = Substitute.For<IDynamicValueConverter>();
        var serializer = new JsonSerializer(testOperationConverter, dynamicValueConverter);

        // Act
        var result = serializer.TryDeserialize<Operation>(json, out var operation, out var exception);

        // Assert
        Assert.True(result);
        Assert.Null(exception);
        Assert.NotNull(operation);
        Assert.Equal(testOperation, operation);
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

    [Fact]
    public void Extension_ShouldReturnJson()
    {
        // Act
        var extension = _serializer.Extension;

        // Assert
        Assert.Equal("json", extension);
    }
}

internal class TestOperationConverter : IOperationConverter
{
    private readonly Operation _operation;

    public TestOperationConverter(Operation operation)
    {
        _operation = operation;
    }

    public Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var operationName = document.RootElement.GetProperty("operation").GetString();

        // Ensure the operation name matches what we expect in the test
        if (operationName == "TestOperation")
            return _operation;

        throw new JsonException("Unexpected operation name");
    }

    public void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}