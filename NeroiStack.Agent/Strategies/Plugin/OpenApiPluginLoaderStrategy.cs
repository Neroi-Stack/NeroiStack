using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public class OpenApiPluginLoaderStrategy : IPluginLoaderStrategy
{
	public bool CanHandle(PluginType type) => type == PluginType.OpenApi;

	public async Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{
		var config = await chatContext.PluginOpenApis.FirstOrDefaultAsync(p => p.PluginId == plugin.Id, ct);
		if (config != null && !string.IsNullOrEmpty(config.Uri))
		{
			await kernel.ImportPluginFromOpenApiAsync(
			plugin.Name ?? $"OpenApi{plugin.Id}",
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