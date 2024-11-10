using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverFlowService(this IServiceCollection services)
    {
        services.AddTransient<IFlow, Flow>();
    }
}
