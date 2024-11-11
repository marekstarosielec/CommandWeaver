using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverFlowService(this IServiceCollection services)
    {
        services.AddTransient<IOperationConditions, OperationConditions>();
        services.AddTransient<IFlow, Flow>();
    }
}
