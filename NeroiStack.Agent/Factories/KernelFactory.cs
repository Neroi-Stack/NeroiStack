using Microsoft.SemanticKernel;
using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Strategies;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Service;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Model;
using NeroiStack.Common.Interface;

namespace NeroiStack.Agent.Factories;

public interface IKernelFactory
{
	Task<(Kernel kernel, dynamic settings)> CreateKernelAsync(IChatContext chatContext, ChatSession session, SupplierEnum supplier, string modelName, CancellationToken ct);
}

public class KernelFactory(IEnumerable<IKernelProviderStrategy> strategies, IEnumerable<IPluginLoaderStrategy> pluginLoaders, IEncryption encryption) : IKernelFactory
{
	public async Task<(Kernel kernel, dynamic settings)> CreateKernelAsync(IChatContext chatContext, ChatSession session, SupplierEnum supplier, string modelName, CancellationToken ct)
	{
		int chatInstanceId = session.ChatInstanceId;
		var keyManageService = new KeyManageService(chatContext, encryption);
		var keys = await keyManageService.GetAllKeysAsync();
		var key = keys.FirstOrDefault(k => k.Supplier == supplier);

		if (supplier != SupplierEnum.Ollama && (key == null || string.IsNullOrWhiteSpace(key.Key)))
		{
			throw new Exception($"API Key not found for supplier {supplier}");
		}

		key ??= new KeyVM { Key = "", Endpoint = "http://localhost:8080/v1/chat/completions" }; // Fallback for local

		var strategy = strategies.FirstOrDefault(s => s.CanHandle(supplier)) ?? throw new NotSupportedException($"Supplier {supplier} not supported");
		var builder = Kernel.CreateBuilder();
		strategy.Configure(builder, modelName, key);

		var kernel = builder.Build();

		await ImportPluginsAsync(kernel, chatContext, session, ct);

		var settings = strategy.CreateExecutionSettings();

		return (kernel, settings);
	}

	private async Task ImportPluginsAsync(Kernel kernel, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		int chatInstanceId = session.ChatInstanceId;
		var agentToolLinks = await chatContext.ChatInstances
			.Join(chatContext.Chats, ci => ci.ChatId, c => c.Id, (ci, c) => new { ci, c })
			.Join(chatContext.ChatAgents, cc => cc.c.Id, ca => ca.ChatId, (cc, ca) => new { cc.ci, cc.c, ca })
			.Join(chatContext.AgentPlugins, cca => cca.ca.AgentId, ap => ap.AgentId, (cca, ap) => new { cca.ci, cca.c, cca.ca, ap })
			.Where(x => x.ci.Id == chatInstanceId)
			.Select(x => x.ap)
			.ToListAsync(ct);

		var pluginIds = agentToolLinks.Select(at => at.PluginId).ToList();
		var plugins = await chatContext.Plugins
			.Where(p => pluginIds.Contains(p.Id) && p.IsEnabled)
			.ToListAsync(ct);

		foreach (var plugin in plugins)
		{
			try
			{
				var loader = pluginLoaders.FirstOrDefault(l => l.CanHandle(plugin.Type));
				if (loader != null)
				{
					await loader.LoadAsync(kernel, plugin, chatContext, session, ct);
				}
				else
				{
					Console.WriteLine($"No loader found for plugin type {plugin.Type}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"error create kernel plugin {plugin.Name}: {ex.Message}");
			}
		}
	}
}
