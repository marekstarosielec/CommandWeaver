using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverOutput(this IServiceCollection services)
    {
        services.AddTransient<IOutput, Output>();
    }
}
