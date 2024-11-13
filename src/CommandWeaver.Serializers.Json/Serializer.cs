using System.Text.Json;
using System.Text.Json.Serialization;

public interface IJsonSerializer : ISerializer { }

/// <inheritdoc />
public class Serializer(IOperationConverter operationConverter, IDynamicValueConverter dynamicValueConverter) : IJsonSerializer
{
    /// <inheritdoc />
    public bool TryDeserialize<T>(string content, out T? result, out Exception? exception) where T : class
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new ConverterWrapper<DynamicValue?>(dynamicValueConverter), new ConverterWrapper<Operation>(operationConverter) }
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

    /// <inheritdoc />
    public bool TrySerialize<T>(T? value, out string? result, out Exception? exception) where T : class
    {
        try
        {
            result = JsonSerializer.Serialize(value, 
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters = { new ConverterWrapper<DynamicValue?>(dynamicValueConverter) },
                    WriteIndented = true,
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