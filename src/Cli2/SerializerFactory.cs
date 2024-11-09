using Microsoft.Extensions.DependencyInjection;

namespace Cli2;

/// <summary>
/// Factory implementation to provide serializers based on format.
/// </summary>
public class SerializerFactory : ISerializerFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider instance used to resolve dependencies.</param>
    public SerializerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ISerializer? GetSerializer(string format) =>
        format.ToLower() switch
        {
            "json" => _serviceProvider.GetRequiredService<Serializer>(),
            _ => default
        };
}