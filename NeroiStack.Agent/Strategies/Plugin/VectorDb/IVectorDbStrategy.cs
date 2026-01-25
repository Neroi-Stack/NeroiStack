using Microsoft.SemanticKernel;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Model;

namespace NeroiStack.Agent.Strategies.Plugin.VectorDb;

public interface IVectorDbStrategy
{
	bool CanHandle(VectorDbType type);
	Task SaveVectorDataAsync(ChPluginVectorDbSearch config, IChatContext chatContext, ChatSession session, string filePath, CancellationToken ct);
	Task LoadVectorDataAsync(ChPluginVectorDbSearch config, IChatContext chatContext, ChatSession session, CancellationToken ct);
	Task DeleteVectorDataAsync(ChPluginVectorDbSearch config, IChatContext chatContext, ChatSession session, CancellationToken ct);
}
