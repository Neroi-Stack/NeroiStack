using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;


namespace NeroiStack.Agent.Data.Configurations
{
	public class ChChatMemoryConfig : IEntityTypeConfiguration<ChChatMemory>
	{
		public void Configure(EntityTypeBuilder<ChChatMemory> builder)
		{
			builder.ToTable("ChChatMemory");
			builder.HasKey(a => a.Id);
			builder.Property(a => a.Id).HasColumnName("Id");
			builder.Property(a => a.ChatInstanceId).HasColumnName("ChatInstanceId");
			builder.Property(a => a.RoleType).HasColumnName("RoleType");
			builder.Property(a => a.Content).HasColumnName("Content").HasColumnType("TEXT");
			builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
		}
	}
}