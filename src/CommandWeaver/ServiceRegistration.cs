using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaver(this IServiceCollection services)
    {
        services.AddTransient<ILoader, Loader>();
        services.AddTransient<ISaver, Saver>();
        services.AddSingleton<IRepositoryStorage, RepositoryStorage>(); //Needs to be singleton, because it stores data.
        services.AddTransient<ICommandWeaver, CommandWeaver>();
        services.AddCommandWeaverSerialization();
        services.AddCommandWeaverFlowService();
        services.AddCommandWeaverVariables();
        services.AddCommandWeaverFileRepository();
        services.AddCommandWeaverEmbeddedRepository();
        services.AddCommandWeaverCommands();
    }
}
