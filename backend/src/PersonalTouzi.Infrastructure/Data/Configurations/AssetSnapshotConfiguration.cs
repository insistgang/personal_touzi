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
        builder.Property(a => a.TotalAssets).HasColumnType("decimal(18,2)");
        builder.Property(a => a.NetValue).HasColumnType("decimal(18,4)");
        builder.Property(a => a.Cash).HasColumnType("decimal(18,2)");
        builder.Property(a => a.PositionsValue).HasColumnType("decimal(18,2)");
        builder.Property(a => a.GainLoss).HasColumnType("decimal(18,2)");
        builder.Property(a => a.GainLossPercent).HasColumnType("decimal(18,4)");
        builder.HasIndex(a => new { a.AccountId, a.SnapshotDate }).IsUnique();
    }
}
