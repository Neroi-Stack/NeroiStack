using Microsoft.Extensions.DependencyInjection;
using NeroiStack.Agent.Strategies.Orchestration;
using NeroiStack.Agent.Strategies.Provider;
using NeroiStack.Agent.Strategies.Plugin;

namespace NeroiStack.Agent.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAgentStrategies(this IServiceCollection services)
	{
		var assembly = typeof(ServiceCollectionExtensions).Assembly;

		// Scan and register IKernelProviderStrategy implementations
		var providerStrategyType = typeof(IKernelProviderStrategy);
		var providerStrategies = assembly.GetTypes()
			.Where(t => providerStrategyType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

		foreach (var type in providerStrategies)
		{
			services.AddScoped(providerStrategyType, type);
		}

		// Scan and register IPluginLoaderStrategy implementations
		var pluginLoaderType = typeof(IPluginLoaderStrategy);
		var pluginLoaders = assembly.GetTypes()
			.Where(t => pluginLoaderType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

		foreach (var type in pluginLoaders)
		{
			services.AddScoped(pluginLoaderType, type);
		}

		// Scan and register IOrchestrationStrategy implementations
		var orchestrationType = typeof(IOrchestrationStrategy);
		var orchestrationStrategies = assembly.GetTypes()
			.Where(t => orchestrationType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

		foreach (var type in orchestrationStrategies)
		{
			services.AddScoped(orchestrationType, type);
		}

		return services;
	}
}
