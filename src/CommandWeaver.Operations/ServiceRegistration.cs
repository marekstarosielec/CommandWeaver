﻿using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddCommandWeaverOperations(this IServiceCollection services)
    {
        services.AddTransient<Output>();
        services.AddTransient<SetVariable>();
        services.AddTransient<Terminate>();
        services.AddTransient<IOperationFactory, OperationFactory>();
    }
}
