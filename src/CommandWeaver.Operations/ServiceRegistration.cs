using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverOperations(this IServiceCollection services)
    {
        services.AddTransient<Output>();
        services.AddTransient<SetVariable>();
        services.AddTransient<Terminate>();
        services.AddTransient<ForEach>();
        services.AddTransient<RestCall>();
        services.AddTransient<RestServer>();
        services.AddTransient<Block>();
        services.AddTransient<ListGroup>();
        services.AddTransient<ExtractFromNameValue>();
        services.AddTransient<IOperationFactory, OperationFactory>();
    }
}
