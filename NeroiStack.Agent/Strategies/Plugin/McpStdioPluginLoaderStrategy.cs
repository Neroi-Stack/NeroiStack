using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class McpStdioPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.McpStdio;
	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginMcpStdios.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Command))
		{
			var clientTransport = new StdioClientTransport(new()
			{
				Name = plugin.Name ?? $"McpStdio{plugin.Id}",
				Command = config.Command,
				Arguments = config.Arguments?.ToArray() ?? []
			});

			var mcpClient = await McpClient.CreateAsync(clientTransport, cancellationToken: CancellationToken.None);
			session.Resources.Add(clientTransport);
			session.Resources.Add(mcpClient);

			var tools = await mcpClient.ListToolsAsync(cancellationToken: ct);
			var validToos = tools.Select(t => t.AsKernelFunction()).ToList();
			if (validToos.Count != 0)
				kernel.Plugins.AddFromFunctions(plugin.Name ?? $"McpStdio{plugin.Id}", validToos);
		}
	}
}