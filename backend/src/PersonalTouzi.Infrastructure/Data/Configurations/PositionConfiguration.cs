using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTouzi.Core.Entities;

namespace PersonalTouzi.Infrastructure.Data.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Type).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(p => p.CostPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.CurrentPrice).HasColumnType("decimal(18,4)");
        builder.HasIndex(p => new { p.AccountId, p.Code });
    }
}
