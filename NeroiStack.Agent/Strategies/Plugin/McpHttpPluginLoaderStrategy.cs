using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class McpHttpPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.McpHttp;
	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginMcpHttps.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Endpoint))
		{
			var clientTransport = new HttpClientTransport(new()
			{
				Name = plugin.Name ?? $"McpHttp{plugin.Id}",
				Endpoint = new Uri(config.Endpoint),
				AdditionalHeaders = string.IsNullOrEmpty(config.ApiKey) ? null : new Dictionary<string, string>
				{
					{ "Authorization", $"Bearer {config.ApiKey}" }
				}
			});

			var mcpClient = await McpClient.CreateAsync(clientTransport, cancellationToken: CancellationToken.None);
			session.Resources.Add(clientTransport);
			session.Resources.Add(mcpClient);

			var tools = await mcpClient.ListToolsAsync(cancellationToken: ct);
			var validToos = tools.Select(t => t.AsKernelFunction()).ToList();
			if (validToos.Count != 0)
				kernel.Plugins.AddFromFunctions(plugin.Name ?? $"McpHttp{plugin.Id}", validToos);
		}
	}
}