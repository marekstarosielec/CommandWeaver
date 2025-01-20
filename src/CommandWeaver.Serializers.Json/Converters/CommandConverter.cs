using System.Text.Json;

public interface ICommandConverter : IConverter<Command>
{ }

public class CommandConverter : ICommandConverter
{
    public Command? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var rootElement = document.RootElement;
        
        // Create a new JsonSerializerOptions without this converter
        var baseOptions = new JsonSerializerOptions(options);
        var converter = baseOptions.Converters.FirstOrDefault(c => c is ConverterWrapper<Command>);
        if (converter != null)
            baseOptions.Converters.Remove(converter);
        
        // Perform base deserialization
        var command = rootElement.Deserialize<Command>(baseOptions);
        if (command == null)
            return null;
        
        // Perform deserialization to DynamicValue so it can be accessed from operations
        var commandAsDynamicValue = rootElement.Deserialize<DynamicValue>(baseOptions);
        if (commandAsDynamicValue == null)
            return null;

        return command with { Source = rootElement.GetRawText(), Definition = commandAsDynamicValue };
    }

    public void Write(Utf8JsonWriter writer, Command value, JsonSerializerOptions options)
    {
        // Write source json - nothing can change command during serialization/deserialization 
        writer.WriteRawValue(value.Source);
    }
}