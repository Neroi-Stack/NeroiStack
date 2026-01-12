using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data.Configurations;

public class ChKeyModelConfig : IEntityTypeConfiguration<ChKeyModel>
{
	public void Configure(EntityTypeBuilder<ChKeyModel> builder)
	{
		builder.ToTable("ChKeyModel");
		builder.HasKey(k => k.Id);
		builder.Property(k => k.Id).HasColumnName("Id");
		builder.Property(k => k.KeyId).HasColumnName("KeyId");
		builder.Property(k => k.ModelId).HasColumnName("ModelId").HasMaxLength(200).IsRequired();

		builder.HasOne(k => k.Key)
			.WithMany(k => k.ModelsNav)
			.HasForeignKey(k => k.KeyId);
	}
}
