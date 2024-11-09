using System.Text.Json;
using System.Text.Json.Serialization;

/// <inheritdoc />
public class Serializer(OperationConverter operationConverter, DynamicValueConverter dynamicValueConverter) : ISerializer
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
                    Converters = { dynamicValueConverter, operationConverter }
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
                    Converters = { dynamicValueConverter },
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