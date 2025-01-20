using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverEmbeddedRepository(this IServiceCollection services)
    {
        services.AddTransient<IEmbeddedRepository, EmbeddedRepository>();
    }
}
