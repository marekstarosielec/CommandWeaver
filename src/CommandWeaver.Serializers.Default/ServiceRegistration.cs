using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverSerialization(this IServiceCollection services)
    {
        services.AddCommandWeaverJsonSerializer();
        services.AddTransient<ISerializerFactory, SerializerFactory>();
    }
}
