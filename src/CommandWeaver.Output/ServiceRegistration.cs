using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverOutput(this IServiceCollection services)
    {
        services.AddTransient<IOutputService, OutputService>();
        //It is singleton because it has styles filled externally and needs to keep it.
        services.AddSingleton<IOutputSettings, OutputSettings>();
    }
}
