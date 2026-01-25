using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginVectorRedisSearchConfig : IEntityTypeConfiguration<ChPluginVectorRedisSearch>
{
	public void Configure(EntityTypeBuilder<ChPluginVectorRedisSearch> builder)
	{
		builder.ToTable("ChPluginVectorRedisSearch");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginVectorDbId).HasColumnName("PluginVectorDbId").IsRequired();
		builder.HasIndex(p => p.PluginVectorDbId);

		builder.Property(p => p.Host).HasColumnName("Host").HasColumnType("TEXT");
		builder.Property(p => p.Port).HasColumnName("Port").IsRequired();
	}
}