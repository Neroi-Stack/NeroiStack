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