using System.Buffers;
using System.Text.Json;

public class CommandConverterTests
{
    [Fact]
    public void Read_ValidJson_ReturnsCommand()
    {
        // Arrange
        var json = "{\"Name\": \"TestCommand\"}";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ConverterWrapper<Command>(new CommandConverter()));

        var converter = new CommandConverter();
        var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(System.Text.Encoding.UTF8.GetBytes(json)));

        // Act
        var result = converter.Read(ref reader, typeof(Command), options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestCommand", result?.Name);
        Assert.NotNull(result?.Definition);
        Assert.Equal(json, result.Source);
    }

    [Fact]
    public void Read_InvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "Invalid JSON";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ConverterWrapper<Command>(new CommandConverter()));
    
        var converter = new CommandConverter();
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(jsonBytes);

        // Act & Assert
        try
        {
            converter.Read(ref reader, typeof(Command), options);
            Assert.Fail("Expected JsonException was not thrown.");
        }
        catch (JsonException)
        {
            Assert.True(true); // JsonException was thrown as expected
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }
    }
    
    [Fact]
    public void Write_WritesRawJson()
    {
        // Arrange
        var command = new Command
        {
            Name = "TestCommand",
            Source = "{\"Name\": \"TestCommand\", \"Parameters\": {\"Key\": \"Value\"}}"
        };

        var options = new JsonSerializerOptions();
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);

        var converter = new CommandConverter();

        // Act
        converter.Write(writer, command, options);
        writer.Flush(); // Ensure all content is written

        // Assert
        var writtenJson = System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
        Assert.Equal(command.Source, writtenJson);
    }
    
    [Fact]
    public void Read_RemovesSpecificConverter()
    {
        // Arrange
        var json = "{\"Name\": \"TestCommand\"}";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ConverterWrapper<Command>(new CommandConverter()));
    
        var converter = new CommandConverter();
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(jsonBytes);

        // Act
        var result = converter.Read(ref reader, typeof(Command), options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestCommand", result?.Name);

        // Confirm original options still contain the ConverterWrapper
        Assert.Contains(options.Converters, c => c is ConverterWrapper<Command>);
    }
}
