using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;


namespace NeroiStack.Agent.Data.Configurations
{
	public class ChAgentConfig : IEntityTypeConfiguration<ChAgent>
	{
		public void Configure(EntityTypeBuilder<ChAgent> builder)
		{
			builder.ToTable("ChAgent");
			builder.HasKey(a => a.Id);
			builder.Property(a => a.Id).HasColumnName("Id");
			builder.Property(a => a.Name).HasColumnName("Name").HasColumnType("TEXT");
			builder.Property(a => a.Description).HasColumnName("Description").HasColumnType("TEXT");
			builder.Property(a => a.Instructions).HasColumnName("Instructions").HasColumnType("TEXT");
			builder.Property(a => a.Kernel).HasColumnName("Kernel").HasColumnType("TEXT");
			builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
			builder.Property(a => a.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
		}
	}
}