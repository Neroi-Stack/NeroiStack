using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;
namespace NeroiStack.Agent.Data.Configurations;

public class ChPluginSqlConfig : IEntityTypeConfiguration<ChPluginSql>
{
	public void Configure(EntityTypeBuilder<ChPluginSql> builder)
	{
		builder.ToTable("ChPluginSql");
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Id).HasColumnName("Id");
		builder.Property(p => p.PluginId).HasColumnName("PluginId").IsRequired();
		builder.HasIndex(p => p.PluginId);

		builder.Property(p => p.Provider).HasColumnName("Provider").HasColumnType("TEXT");
		builder.Property(p => p.ConnectionString).HasColumnName("ConnectionString").HasColumnType("TEXT");
	}
}