using Microsoft.SemanticKernel;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using ModelContextProtocol.Client;
using NeroiStack.Agent.Data;

namespace NeroiStack.Agent.Strategies;

public interface IPluginLoaderStrategy
{
	bool CanHandle(PluginType type);
	Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, CancellationToken ct);
}

public class OpenApiPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.OpenApi;

	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, CancellationToken ct)
	{
		var config = await chatContext.PluginOpenApis.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Uri))
		{
			await kernel.ImportPluginFromOpenApiAsync(
			plugin.Name ?? $"OpenApi_{plugin.Id}",
			uri: new Uri(config.Uri),
			executionParameters: new OpenApiFunctionExecutionParameters
			{
				IgnoreNonCompliantErrors = true,
				EnableDynamicPayload = true,
				EnablePayloadNamespacing = true
			},
			cancellationToken: ct);
		}
	}
}

public class McpStdioPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.McpStdio;
	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, CancellationToken ct)
	{
		var config = await chatContext.PluginMcpStdios.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Command))
		{
			var clientTransport = new StdioClientTransport(new()
			{
				Command = config.Command,
				Arguments = config.Arguments?.ToArray() ?? []
			});

			var mcpClient = await McpClient.CreateAsync(clientTransport, cancellationToken: CancellationToken.None);
			var tools = await mcpClient.ListToolsAsync(cancellationToken: ct);
			var validToos = tools.Select(t => t.AsKernelFunction()).ToList();
			if (validToos.Any())
				kernel.Plugins.AddFromFunctions(plugin.Name ?? $"McpStdio_{plugin.Id}", validToos);
		}
	}
}

public class McpHttpPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.McpHttp;
	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, CancellationToken ct)
	{
		var config = await chatContext.PluginMcpHttps.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Endpoint))
		{
			var clientTransport = new HttpClientTransport(new()
			{
				Endpoint = new Uri(config.Endpoint),
				AdditionalHeaders = string.IsNullOrEmpty(config.ApiKey) ? null : new Dictionary<string, string>
				{
					{ "Authorization", $"Bearer {config.ApiKey}" }
				}
			});

			var mcpClient = await McpClient.CreateAsync(clientTransport, cancellationToken: CancellationToken.None);
			var tools = await mcpClient.ListToolsAsync(cancellationToken: ct);
			var validToos = tools.Select(t => t.AsKernelFunction()).ToList();
			if (validToos.Any())
				kernel.Plugins.AddFromFunctions(plugin.Name ?? $"McpHttp_{plugin.Id}", validToos);
		}
	}
}
