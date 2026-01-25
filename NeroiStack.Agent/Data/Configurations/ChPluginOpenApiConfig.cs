using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginOpenApiConfig : IEntityTypeConfiguration<ChPluginOpenApi>
{
	public void Configure(EntityTypeBuilder<ChPluginOpenApi> builder)
	{
		builder.ToTable("ChPluginOpenApi");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		// Relationship? usually we'd have a navigation property, but let's just index it
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.AuthType).HasColumnName("AuthType");
		builder.Property(p => p.ApiKey).HasColumnName("ApiKey").HasColumnType("TEXT");
		builder.Property(p => p.BearerToken).HasColumnName("BearerToken").HasColumnType("TEXT");
		builder.Property(p => p.Uri).HasColumnName("Uri").HasColumnType("TEXT");
		builder.Property(p => p.FilePath).HasColumnName("FilePath").HasColumnType("TEXT");
	}
}