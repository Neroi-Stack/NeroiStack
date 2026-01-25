using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginVectorDbSearchConfig : IEntityTypeConfiguration<ChPluginVectorDbSearch>
{
	public void Configure(EntityTypeBuilder<ChPluginVectorDbSearch> builder)
	{
		builder.ToTable("ChPluginVectorDbSearch");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);
		builder.Property(p => p.DbType).HasColumnName("DbType").IsRequired();
		builder.Property(p => p.EmbeddedKeyId).HasColumnName("EmbeddedKeyId").HasColumnType("TEXT");
		builder.Property(p => p.ModelName).HasColumnName("ModelName").HasColumnType("TEXT");
		builder.Property(p => p.Dimension).HasColumnName("Dimension").IsRequired();
	}
}