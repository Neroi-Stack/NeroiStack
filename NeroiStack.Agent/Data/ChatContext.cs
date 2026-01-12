using Microsoft.EntityFrameworkCore;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data;

public class ChatContext(DbContextOptions<ChatContext> options) : DbContext(options), IChatContext
{
	public DbSet<ChChat> Chats { get; set; }
	public DbSet<ChChatInstance> ChatInstances { get; set; }
	public DbSet<ChChatAgent> ChatAgents { get; set; }
	public DbSet<ChChatHandoff> ChatHandoffs { get; set; }
	public DbSet<ChAgent> Agents { get; set; }
	public DbSet<ChChatMemory> ChatMemories { get; set; }
	// public DbSet<ChConfig> Configs { get; set; }
	public DbSet<ChKey> Keys { get; set; }
	public DbSet<ChKeyModel> KeyModels { get; set; }
	public DbSet<ChPlugin> Plugins { get; set; }
	public DbSet<ChPluginOpenApi> PluginOpenApis { get; set; }
	public DbSet<ChPluginMcpHttp> PluginMcpHttps { get; set; }
	public DbSet<ChPluginMcpStdio> PluginMcpStdios { get; set; }
	public DbSet<ChPluginMcpHttpStreamable> PluginMcpHttpStreamables { get; set; }
	public DbSet<ChAgentPlugin> AgentPlugins { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return base.SaveChangesAsync(cancellationToken);
	}
}