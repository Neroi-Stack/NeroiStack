using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.KernelFunc;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class SqlAgentToolLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.SqlAgentTool;

	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginSqls.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Provider) && !string.IsNullOrEmpty(config.ConnectionString))
		{
			try
			{
				var pluginName = plugin.Name ?? $"SqlAgent{plugin.Id}";
				var sqlAgent = new SqlAgentPlugin(config.ConnectionString, config.Provider);
				kernel.Plugins.AddFromObject(sqlAgent, pluginName);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"error loading sql agent plugin {plugin.Name}: {ex.Message}");
			}
		}
	}
}