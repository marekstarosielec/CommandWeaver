using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverJsonSerializer(this IServiceCollection services)
    {
        services.AddTransient<IDynamicValueConverter, DynamicValueConverter>();
        services.AddTransient<IOperationConverter, OperationConverter>();
        services.AddTransient<IJsonSerializer, JsonSerializer>();
    }
}
