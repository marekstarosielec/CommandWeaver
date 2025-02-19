using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <inheritdoc />
public class JsonSerializer(IDynamicValueConverter dynamicValueConverter, ICommandConverter commandConverter) : IJsonSerializer
{
    /// <inheritdoc />
    public bool TryDeserialize<T>(string content, out T? result, out Exception? exception) where T : class
    {
        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<T>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new ConverterWrapper<DynamicValue?>(dynamicValueConverter), new ConverterWrapper<Command>(commandConverter) }
                });
            exception = null;
            return true;
        }
        catch (Exception ex)
        {
            result = null;
            exception = ex;
            Console.WriteLine(content);
            return false;
        }
    }

    /// <inheritdoc />
    public bool TrySerialize<T>(T? value, out string? result, out Exception? exception) where T : class
    {
        try
        {
            result = System.Text.Json.JsonSerializer.Serialize(value, 
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters = { new ConverterWrapper<DynamicValue?>(dynamicValueConverter), new ConverterWrapper<Command>(commandConverter) },
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Prevents escaping `&`, `<`, `>`, `+`
                });
            exception = null;
            return true;
        }
        catch (Exception ex)
        {
            result = null;
            exception = ex;
            return false;
        }
    }
}