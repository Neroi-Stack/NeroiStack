#pragma warning disable SKEXP0050
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class BingSearchPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.BingSearch;
	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginBingSearches.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.ApiKey))
		{
			var bingSearchClient = new BingTextSearch(config.ApiKey);
			session.Resources.Add(bingSearchClient);

			var name = plugin.Name ?? $"BingSearch{plugin.Id}";
			var searchPlugin = bingSearchClient.CreateWithSearch(name);
			kernel.Plugins.Add(searchPlugin);
		}
	}
}