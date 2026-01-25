#pragma warning disable SKEXP0050
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using NeroiStack.Agent.Data;
using NeroiStack.Agent.Data.Entities;
using NeroiStack.Agent.Enum;
using NeroiStack.Agent.Model;
using NeroiStack.Agent.Service;
using NeroiStack.Common.Interface;
using StackExchange.Redis;

namespace NeroiStack.Agent.Strategies.Plugin.VectorDb;

public class RedisStrategy(IEncryption encryption) : IVectorDbStrategy
{
	private readonly IEncryption _encryption = encryption;

	public bool CanHandle(VectorDbType type)
	{
		return type == VectorDbType.Redis;
	}

	public async Task SaveVectorDataAsync(ChPluginVectorDbSearch config, IChatContext chatContext, ChatSession session, string filePath, CancellationToken ct)
	{

	}

	public async Task LoadVectorDataAsync(ChPluginVectorDbSearch config, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{

	}

	public async Task DeleteVectorDataAsync(ChPluginVectorDbSearch config, IChatContext chatContext, ChatSession session, CancellationToken ct)
	{

	}
}