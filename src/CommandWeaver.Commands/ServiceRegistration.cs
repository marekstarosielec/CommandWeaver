using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverCommands(this IServiceCollection services)
    {
        services.AddSingleton<ICommands, Commands>();
    }
}
