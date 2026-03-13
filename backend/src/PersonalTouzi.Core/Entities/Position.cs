namespace PersonalTouzi.Core.Entities;

public class Position
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "stock";  // stock, fund, bond
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketValue => Quantity * CurrentPrice;
    public decimal ProfitLoss => MarketValue - (Quantity * CostPrice);
    public decimal ProfitLossPercent => CostPrice > 0 ? (CurrentPrice - CostPrice) / CostPrice * 100 : 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
