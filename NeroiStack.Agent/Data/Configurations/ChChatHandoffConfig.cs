using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data.Configurations;

public class ChChatHandoffConfig : IEntityTypeConfiguration<ChChatHandoff>
{
	public void Configure(EntityTypeBuilder<ChChatHandoff> builder)
	{
		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.Chat)
			.WithMany()
			.HasForeignKey(x => x.ChatId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.FromAgent)
			.WithMany()
			.HasForeignKey(x => x.FromAgentId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(x => x.ToAgent)
			.WithMany()
			.HasForeignKey(x => x.ToAgentId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}