using System.Text.Json;
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
                    PropertyNameCaseInsensitive = true, Converters = { new ObjectToPureTypeConverter(), operationConverter },
                    WriteIndented = true,
                });
        }
        catch
        {
            // Return null if deserialization fails
            return null;
        }
    }
}