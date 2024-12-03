using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverVariables(this IServiceCollection services)
    {
        services.AddTransient<IReader, Reader>();
        services.AddTransient<IWriter, Writer>();
        services.AddSingleton<IVariableStorage, VariableStorage>(); //Variable storage is single instance for whole execution.
        services.AddTransient<IVariableService, VariableService>();
    }
}
