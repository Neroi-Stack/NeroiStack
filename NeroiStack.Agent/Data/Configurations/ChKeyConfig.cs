using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeroiStack.Agent.Data.Entities;

namespace NeroiStack.Agent.Data.Configurations;

public class ChKeyConfig : IEntityTypeConfiguration<ChKey>
{
	public void Configure(EntityTypeBuilder<ChKey> builder)
	{
		builder.ToTable("ChKey");
		builder.HasKey(k => k.Id);
		builder.Property(k => k.Id).HasColumnName("Id");
		builder.Property(k => k.Supplier).HasColumnName("Supplier").IsRequired();
		builder.Property(k => k.EncryptedKey).HasColumnName("EncryptedKey").HasColumnType("TEXT").IsRequired();
		builder.Property(k => k.KeyType).HasColumnName("KeyType").IsRequired();
		builder.Property(k => k.Endpoint).HasColumnName("Endpoint").HasColumnType("TEXT");

		builder.HasMany(k => k.ModelsNav)
			.WithOne(m => m.Key)
			.HasForeignKey(m => m.KeyId);
	}
}
