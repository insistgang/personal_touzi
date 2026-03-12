namespace PersonalTouzi.Core.Entities;

public class AssetSnapshot
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal TotalAsset { get; set; }
    public decimal Cash { get; set; }
    public decimal PositionValue { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal DailyProfit { get; set; }
    public decimal DailyProfitPercent { get; set; }
    public DateTime SnapshotDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
