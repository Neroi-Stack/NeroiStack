using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data.Configurations;

public class ChAgentToolConfig : IEntityTypeConfiguration<ChAgentPlugin>
{
	public void Configure(EntityTypeBuilder<ChAgentPlugin> builder)
	{
		builder.ToTable("ChAgentTool");
		builder.HasKey(at => new { at.AgentId, at.PluginId });
		builder.Property(at => at.AgentId).HasColumnName("AgentId");
		builder.Property(at => at.PluginId).HasColumnName("PluginId");
	}
}
