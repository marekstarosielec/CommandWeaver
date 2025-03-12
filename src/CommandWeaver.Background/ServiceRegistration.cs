using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverBackground(this IServiceCollection services)
    {
        //Stores list of tasks inside.
        services.AddSingleton<IBackgroundService, BackgroundService>();
    }
}
