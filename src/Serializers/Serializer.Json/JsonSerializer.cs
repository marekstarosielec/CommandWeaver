using System.Text.Json;
using System.Text.Json.Serialization;
using Serializer.Abstractions;

namespace Serializer.Json;

/// <summary>
/// JSON serializer implementation of <see cref="ISerializer"/>.
/// </summary>
public class JsonSerializer(OperationConverter operationConverter) : ISerializer
{
    /// <inheritdoc />
    public T? Deserialize<T>(string content) where T : class
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, Converters = { new DynamicValueConverter(), operationConverter },
                    WriteIndented = true,
                });
        }
        catch
        {
            // Return null if deserialization fails
            return null;
        }
    }

    public string Serialize<T>(T? value) where T : class
    {
        return System.Text.Json.JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new DynamicValueConverter(), operationConverter },
            WriteIndented = true,
        });
    }
}