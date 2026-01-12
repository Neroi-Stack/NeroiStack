using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data;

public interface IChatContext
{
	public DbSet<ChChat> Chats { get; }
	public DbSet<ChChatInstance> ChatInstances { get; }
	public DbSet<ChChatAgent> ChatAgents { get; }
	public DbSet<ChChatHandoff> ChatHandoffs { get; }
	public DbSet<ChAgent> Agents { get; }
	public DbSet<ChChatMemory> ChatMemories { get; }
	// public DbSet<ChConfig> Configs { get; }
	public DbSet<ChKey> Keys { get; }
	public DbSet<ChKeyModel> KeyModels { get; }
	public DbSet<ChPlugin> Plugins { get; }
	public DbSet<ChPluginOpenApi> PluginOpenApis { get; }
	public DbSet<ChPluginMcpHttp> PluginMcpHttps { get; }
	public DbSet<ChPluginMcpStdio> PluginMcpStdios { get; }
	public DbSet<ChPluginMcpHttpStreamable> PluginMcpHttpStreamables { get; }
	public DbSet<ChAgentPlugin> AgentPlugins { get; }
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}