using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverInput(this IServiceCollection services)
    {
        services.AddTransient<IInputService, InputService>();
    }
}
