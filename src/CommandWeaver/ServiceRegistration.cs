using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaver(this IServiceCollection services)
    {
        services.AddTransient<ILoader, Loader>();
        services.AddTransient<ISaver, Saver>();
        services.AddSingleton<IRepositoryElementStorage, RepositoryElementStorage>(); //Needs to be singleton, because it stores data.
        services.AddTransient<ICommandWeaver, CommandWeaver>();
        services.AddTransient<IOutputWriter, SpectreConsoleOutput>();
        services.AddCommandWeaverJsonSerializer();
        services.AddCommandWeaverFlow();
        services.AddCommandWeaverOutput();
        services.AddCommandWeaverOperations();
        services.AddCommandWeaverVariables();
        services.AddCommandWeaverFileRepository();
        services.AddCommandWeaverEmbeddedRepository();
        services.AddCommandWeaverCommands();
        services.AddCommandWeaverResource();
    }
}
