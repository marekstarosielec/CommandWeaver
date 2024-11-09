using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverJsonSerializer(this IServiceCollection services)
    {
        services.AddTransient<OperationConverter>();
        services.AddTransient<DynamicValueConverter>();
        services.AddTransient<Serializer>();
    }
}
