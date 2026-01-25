using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

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