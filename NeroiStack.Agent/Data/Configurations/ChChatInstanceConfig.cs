using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;


namespace NeroiStack.Agent.Data.Configurations
{
	public class ChChatInstanceConfig : IEntityTypeConfiguration<ChChatInstance>
	{
		public void Configure(EntityTypeBuilder<ChChatInstance> builder)
		{
			builder.ToTable("ChChatInstance");
			builder.HasKey(a => a.Id);
			builder.Property(a => a.Id).HasColumnName("Id");
			builder.Property(a => a.ChatId).HasColumnName("ChatId");
			builder.Property(a => a.ChatInstanceName).HasColumnName("ChatInstanceName");
			builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
		}
	}
}