using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverResource(this IServiceCollection services)
    {
        services.AddSingleton<IResourceService, ResourceService>(); //Needs to be singleton, because it stores data.
    }
}
