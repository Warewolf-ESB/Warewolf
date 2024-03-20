using Blazored.LocalStorage;
using Elsa.Studio.Login.BlazorWasm.Services;
using Elsa.Studio.Login.Contracts;
using Elsa.Studio.Login.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Studio.Login.BlazorWasm.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core services with Blazor Server implementations.
    /// </summary>
    public static IServiceCollection AddLoginModule(this IServiceCollection services)
    {
        services.AddLoginModuleCore();
        
        // Register Blazored LocalStorage.
        services.AddBlazoredLocalStorage();
        
        // Register JWT services.
        services.AddSingleton<IJwtParser, BlazorWasmJwtParser>();
        services.AddScoped<IJwtAccessor, BlazorWasmJwtAccessor>();
        
        return services;
    }
}