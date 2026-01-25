#pragma warning disable SKEXP0050
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class GoogleSearchPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.GoogleSearch;
	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginGoogleSearches.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.ApiKey) && !string.IsNullOrEmpty(config.SearchEngineId))
		{
			var name = plugin.Name ?? $"GoogleSearch{plugin.Id}";

			var textSearch = new GoogleTextSearch(
				searchEngineId: config.SearchEngineId,
				apiKey: config.ApiKey);

			session.Resources.Add(textSearch);

			var searchPlugin = textSearch.CreateWithSearch(name);
			kernel.Plugins.Add(searchPlugin);
		}
	}
}