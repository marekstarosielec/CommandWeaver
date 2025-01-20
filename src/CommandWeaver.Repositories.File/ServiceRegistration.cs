using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverFileRepository(this IServiceCollection services)
    {
        services.AddTransient<IPhysicalFileProvider, PhysicalFileProvider>();
        services.AddTransient<IRepository, FileRepository>();
    }
}
