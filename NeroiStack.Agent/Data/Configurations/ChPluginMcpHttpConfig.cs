using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginMcpHttpConfig : IEntityTypeConfiguration<ChPluginMcpHttp>
{
	public void Configure(EntityTypeBuilder<ChPluginMcpHttp> builder)
	{
		builder.ToTable("ChPluginMcpHttp");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.Endpoint).HasColumnName("Endpoint").HasColumnType("TEXT");
		builder.Property(p => p.ApiKey).HasColumnName("ApiKey").HasColumnType("TEXT");
	}
}