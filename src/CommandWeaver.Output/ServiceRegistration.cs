using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverOutput(this IServiceCollection services)
    {
        //It is singletin because it has styles filled externally and needs to keep it.
        services.AddSingleton<IOutput, Output>();
    }
}
