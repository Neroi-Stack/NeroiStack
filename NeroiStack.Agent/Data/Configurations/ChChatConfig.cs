using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;


namespace NeroiStack.Agent.Data.Configurations
{
	public class ChChatConfig : IEntityTypeConfiguration<ChChat>
	{
		public void Configure(EntityTypeBuilder<ChChat> builder)
		{
			builder.ToTable("ChChat");
			builder.HasKey(a => a.Id);
			builder.Property(a => a.Id).HasColumnName("Id");
			builder.Property(a => a.Name).HasColumnName("Name").HasColumnType("TEXT");
			builder.Property(a => a.IsStreamable).HasColumnName("IsStreamable").HasDefaultValue(true);
			builder.Property(a => a.AgentOrchestrationType).HasColumnName("AgentOrchestrationType").IsRequired();
			builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
			builder.Property(a => a.IsEnabled).HasColumnName("IsEnabled").HasDefaultValue(true);
		}
	}
}