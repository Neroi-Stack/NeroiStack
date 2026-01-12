using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;


namespace NeroiStack.Agent.Data.Configurations
{
	public class ChChatAgentConfig : IEntityTypeConfiguration<ChChatAgent>
	{
		public void Configure(EntityTypeBuilder<ChChatAgent> builder)
		{
			builder.ToTable("ChChatAgent");
			builder.HasKey(ca => new { ca.ChatId, ca.AgentId });
			builder.Property(ca => ca.ChatId).HasColumnName("ChatId");
			builder.Property(ca => ca.AgentId).HasColumnName("AgentId");
			builder.Property(ca => ca.Order).HasColumnName("Order").HasDefaultValue(0);
			builder.Property(ca => ca.IsPrimary).HasColumnName("IsPrimary").HasDefaultValue(false);
		}
	}
}