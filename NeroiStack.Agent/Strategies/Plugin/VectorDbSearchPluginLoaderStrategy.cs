using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Strategies.Plugin.VectorDb;

namespace NeroiStack.Agent.Strategies.Plugin;

public class VectorDbSearchPluginLoaderStrategy(IEnumerable<IVectorDbStrategy> vectorDbStrategies) : IPluginLoaderStrategy
{
	private readonly IEnumerable<IVectorDbStrategy> _vectorDbStrategies = vectorDbStrategies ?? [];

	public bool CanHandle(PluginType type) => type == PluginType.VectorDbSearch;

	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginVectorDbSearches.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null)
		{
			var strategy = _vectorDbStrategies.FirstOrDefault(s => s.CanHandle(config.DbType));
			if (strategy != null)
			{
				await strategy.LoadVectorDataAsync(config, chatContext, session, ct);
			}
			else
			{
				Console.WriteLine($"No vector DB strategy registered for {config.DbType}");
			}
		}
	}
}