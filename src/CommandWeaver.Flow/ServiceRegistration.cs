using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverFlow(this IServiceCollection services)
    {
        services.AddTransient<IFlow, Flow>();
    }
}
