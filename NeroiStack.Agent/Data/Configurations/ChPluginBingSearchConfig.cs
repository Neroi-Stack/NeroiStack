using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginBingSearchConfig : IEntityTypeConfiguration<ChPluginBingSearch>
{
	public void Configure(EntityTypeBuilder<ChPluginBingSearch> builder)
	{
		builder.ToTable("ChPluginBingSearch");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.ApiKey).HasColumnName("ApiKey").HasColumnType("TEXT");
	}
}
