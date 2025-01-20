using Microsoft.Extensions.DependencyInjection;

internal static class ServicesConfiguration
{
    internal static void Install(IServiceCollection services)
    {
        // Register services
        services.AddCommandWeaver();
        services.AddTransient<Parser>();
        
        // Register the main application entry point
        services.AddTransient<App>();
    }
}
