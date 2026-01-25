using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginGoogleSearchConfig : IEntityTypeConfiguration<ChPluginGoogleSearch>
{
	public void Configure(EntityTypeBuilder<ChPluginGoogleSearch> builder)
	{
		builder.ToTable("ChPluginGoogleSearch");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.SearchEngineId).HasColumnName("SearchEngineId").HasColumnType("TEXT");
		builder.Property(p => p.ApiKey).HasColumnName("ApiKey").HasColumnType("TEXT");
	}
}
