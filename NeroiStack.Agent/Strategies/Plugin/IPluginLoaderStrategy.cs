using Microsoft.SemanticKernel;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin;

public interface IPluginLoaderStrategy
{
	bool CanHandle(PluginType type);
	Task LoadAsync(Kernel kernel, ChPlugin plugin, IChatContext chatContext, ChatSession session, CancellationToken ct);
}
