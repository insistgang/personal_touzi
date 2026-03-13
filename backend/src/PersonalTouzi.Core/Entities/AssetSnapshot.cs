namespace PersonalTouzi.Core.Entities;

public class AssetSnapshot
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal NetValue { get; set; } = 1.0m;
    public decimal Cash { get; set; }
    public decimal PositionsValue { get; set; }
    public decimal GainLoss { get; set; }
    public decimal GainLossPercent { get; set; }
    public DateTime SnapshotDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
