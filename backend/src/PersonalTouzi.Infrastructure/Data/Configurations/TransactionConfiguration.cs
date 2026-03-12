using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTouzi.Core.Entities;

namespace PersonalTouzi.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Type).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Price).HasColumnType("decimal(18,4)");
        builder.Property(t => t.Remark).HasMaxLength(500);
        builder.HasIndex(t => new { t.AccountId, t.TransactionDate });
    }
}
