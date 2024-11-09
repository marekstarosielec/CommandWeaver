using Microsoft.Extensions.DependencyInjection;

/// <inheritdoc />
public class SerializerFactory(IServiceProvider serviceProvider) : ISerializerFactory
{
    /// <inheritdoc />
    public ISerializer? GetSerializer(string format) =>
        format.ToLower() switch
        {
            "json" => serviceProvider.GetRequiredService<Serializer>(),
            _ => default
        };
}