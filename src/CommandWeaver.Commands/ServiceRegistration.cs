using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverCommands(this IServiceCollection services)
    {
        services.AddSingleton<ICommandService, CommandService>();
        services.AddTransient<IValidator, Validator>();
        services.AddTransient<ICommandParameterResolver, CommandParameterResolver>();
        services.AddTransient<IConditionsService, ConditionsService>();
        services.AddTransient<ICommandMetadataService, CommandMetadataService>();
        services.AddTransient<IOperationParameterResolver, OperationParameterResolver>();
    }
}
