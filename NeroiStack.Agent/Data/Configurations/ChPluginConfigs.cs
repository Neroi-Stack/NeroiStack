using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginConfig : IEntityTypeConfiguration<ChPlugin>
{
	public void Configure(EntityTypeBuilder<ChPlugin> builder)
	{
		builder.ToTable("ChPlugin");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.Type).HasColumnName("Type").IsRequired();
		builder.Property(p => p.Name).HasColumnName("Name").HasColumnType("TEXT");
		builder.Property(p => p.Description).HasColumnName("Description").HasColumnType("TEXT");
		builder.Property(p => p.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
	}
}

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

public class ChPluginMcpStdioConfig : IEntityTypeConfiguration<ChPluginMcpStdio>
{
	public void Configure(EntityTypeBuilder<ChPluginMcpStdio> builder)
	{
		builder.ToTable("ChPluginMcpStdio");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.Command).HasColumnName("Command").HasColumnType("TEXT");

		// Convert List<string> to JSON string
		builder.Property(p => p.Arguments)
			.HasConversion(
				v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
				v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)
			)
			.HasColumnName("Arguments")
			.HasColumnType("TEXT");
	}
}

public class ChPluginMcpHttpStreamableConfig : IEntityTypeConfiguration<ChPluginMcpHttpStreamable>
{
	public void Configure(EntityTypeBuilder<ChPluginMcpHttpStreamable> builder)
	{
		builder.ToTable("ChPluginMcpHttpStreamable");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.Endpoint).HasColumnName("Endpoint").HasColumnType("TEXT");
		builder.Property(p => p.ApiKey).HasColumnName("ApiKey").HasColumnType("TEXT");
	}
}
