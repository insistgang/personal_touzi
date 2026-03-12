using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalTouzi.Core.Entities;

namespace PersonalTouzi.Infrastructure.Data.Configurations;

public class AssetSnapshotConfiguration : IEntityTypeConfiguration<AssetSnapshot>
{
    public void Configure(EntityTypeBuilder<AssetSnapshot> builder)
    {
        builder.ToTable("AssetSnapshots");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TotalAsset).HasColumnType("decimal(18,2)");
        builder.Property(a => a.Cash).HasColumnType("decimal(18,2)");
        builder.Property(a => a.PositionValue).HasColumnType("decimal(18,2)");
        builder.Property(a => a.TotalProfit).HasColumnType("decimal(18,2)");
        builder.Property(a => a.DailyProfit).HasColumnType("decimal(18,2)");
        builder.Property(a => a.DailyProfitPercent).HasColumnType("decimal(18,4)");
        builder.HasIndex(a => new { a.AccountId, a.SnapshotDate }).IsUnique();
    }
}
