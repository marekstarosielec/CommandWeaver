using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public class SerializerFactory(IJsonSerializer jsonSerializer) : ISerializerFactory
{
    public ISerializer GetDefaultSerializer(out string format)
    {
        format = "json";
        return jsonSerializer;
    }

    /// <inheritdoc />
    public ISerializer? GetSerializer(string format) =>
        format.ToLower() switch
        {
            "json" => jsonSerializer,
            _ => default
        };
}